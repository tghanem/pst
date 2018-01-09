﻿using pst.encodables.ndb;
using pst.impl.decoders.ltp.hn;
using pst.impl.decoders.messaging;
using pst.impl.decoders.ndb;
using pst.impl.decoders.ndb.blocks;
using pst.impl.decoders.ndb.blocks.data;
using pst.impl.ltp;
using pst.impl.ltp.pc;
using pst.impl.ltp.tc;
using pst.impl.messaging;
using pst.interfaces;
using pst.interfaces.ltp;
using pst.interfaces.ltp.pc;
using pst.interfaces.ltp.tc;
using pst.interfaces.ndb;
using System.IO;

namespace pst
{
    public partial class PSTFile
    {
        private static ITableContextBasedPropertyReader CreateTagBasedTableContextBasedPropertyReader(
            Stream dataStream,
            ICache<NID[], NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new TableContextBasedPropertyReader(
                    CreateTagBasedRowMatrixReader(dataStream, nodeEntryCache, dataBlockEntryCache),
                    CreatePropertyValueProcessor(dataStream, nodeEntryCache, dataBlockEntryCache));
        }

        private static IPropertyNameToIdMap CreatePropertyIdToNameMap(
            Stream dataStream,
            ICache<NID[], NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new PropertyNameToIdMap(
                    new NAMEIDDecoder(),
                    CreatePropertyContextBasedPropertyReader(dataStream, nodeEntryCache, dataBlockEntryCache));
        }

        private static IPropertyContextBasedPropertyReader CreatePropertyContextBasedPropertyReader(
            Stream dataStream,
            ICache<NID[], NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new PropertyContextBasedPropertyReader(
                    CreatePropertyIdBasedBTreeOnHeapReader(dataStream, nodeEntryCache, dataBlockEntryCache),
                    CreatePropertyValueProcessor(dataStream, nodeEntryCache, dataBlockEntryCache));
        }

        private static IPropertyValueReader CreatePropertyValueProcessor(
            Stream dataStream,
            ICache<NID[], NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new PropertyValueReader(
                    new HNIDDecoder(
                        new HIDDecoder(),
                        new NIDDecoder()),
                    new ExternalDataBlockDecoder(
                        new BlockTrailerDecoder(
                            new BIDDecoder()),
                        CreateBlockEncoding(dataStream)),
                    CreateDataBlockReader(dataStream, dataBlockEntryCache),
                    CreateDataBlockEntryFinder(dataStream, dataBlockEntryCache),
                    CreateNodeEntryFinder(dataStream, nodeEntryCache, dataBlockEntryCache),
                    CreateHeapOnNodeReader(dataStream, nodeEntryCache, dataBlockEntryCache));
        }
    }
}
