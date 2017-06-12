﻿using pst.core;
using pst.encodables.ltp.hn;
using pst.encodables.ndb;
using pst.encodables.ndb.btree;
using pst.interfaces;
using pst.interfaces.ltp;
using pst.interfaces.ltp.bth;
using pst.interfaces.ltp.hn;
using pst.interfaces.ltp.pc;
using pst.utilities;

namespace pst.impl.ltp.pc
{
    class PCBasedPropertyReader : IPCBasedPropertyReader
    {
        private readonly IHeapOnNodeReader heapOnNodeReader;
        private readonly IDecoder<HNID> hnidDecoder;
        private readonly IBTreeOnHeapReader<PropertyId> bthReader;
        private readonly IPropertyTypeMetadataProvider propertyTypeMetadataProvider;

        private readonly IMapper<NID, LNBTEntry> nodeIdToLNBTEntryMapping;
        private readonly IMapper<BID, LBBTEntry> blockIdToLBBTEntryMapping;

        public PCBasedPropertyReader(
            IHeapOnNodeReader heapOnNodeReader,
            IDecoder<HNID> hnidDecoder,
            IBTreeOnHeapReader<PropertyId> bthReader,
            IPropertyTypeMetadataProvider propertyTypeMetadataProvider,
            IMapper<NID, LNBTEntry> nodeIdToLNBTEntryMapping,
            IMapper<BID, LBBTEntry> blockIdToLBBTEntryMapping)
        {
            this.heapOnNodeReader = heapOnNodeReader;
            this.hnidDecoder = hnidDecoder;
            this.bthReader = bthReader;
            this.propertyTypeMetadataProvider = propertyTypeMetadataProvider;
            this.nodeIdToLNBTEntryMapping = nodeIdToLNBTEntryMapping;
            this.blockIdToLBBTEntryMapping = blockIdToLBBTEntryMapping;
        }

        public Maybe<PropertyValue> ReadProperty(NID nodeId, PropertyTag propertyTag)
        {
            var nbtEntry =
                nodeIdToLNBTEntryMapping.Map(nodeId);

            var bbtEntry =
                blockIdToLBBTEntryMapping.Map(nbtEntry.DataBlockId);

            var dataBlockId =
                bbtEntry.BlockReference.BlockId;

            var dataRecord =
                bthReader.ReadDataRecord(dataBlockId, propertyTag.Id);

            if (dataRecord.HasNoValue)
                return Maybe<PropertyValue>.NoValue();

            if (propertyTypeMetadataProvider.IsFixedLength(propertyTag.Type))
            {
                var size =
                    propertyTypeMetadataProvider.GetFixedLengthTypeSize(propertyTag.Type);

                if (size <= 4)
                {
                    return new PropertyValue(dataRecord.Value.Data.Take(2, 4));
                }
                else
                {
                    var hnid = hnidDecoder.Decode(dataRecord.Value.Data.Take(2, 4));

                    var heapItem = heapOnNodeReader.GetHeapItem(dataBlockId, hnid.HID);

                    return new PropertyValue(heapItem);
                }
            }
            else if (propertyTypeMetadataProvider.IsVariableLength(propertyTag.Type))
            {
                var hnid = hnidDecoder.Decode(dataRecord.Value.Data.Take(2, 4));

                if (hnid.IsHID)
                {
                    if (hnid.HID.Index == 0)
                    {
                        return Maybe<PropertyValue>.OfValue(PropertyValue.Empty);
                    }

                    var heapItem = heapOnNodeReader.GetHeapItem(dataBlockId, hnid.HID);

                    return new PropertyValue(heapItem);
                }
            }

            return Maybe<PropertyValue>.NoValue();
        }
    }
}
