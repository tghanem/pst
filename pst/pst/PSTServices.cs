﻿using pst.encodables.ndb;
using pst.encodables.ndb.blocks.data;
using pst.encodables.ndb.blocks.subnode;
using pst.encodables.ndb.btree;
using pst.impl.btree;
using pst.impl.converters;
using pst.impl.decoders.ltp.bth;
using pst.impl.decoders.ltp.hn;
using pst.impl.decoders.ltp.tc;
using pst.impl.decoders.messaging;
using pst.impl.decoders.ndb;
using pst.impl.decoders.ndb.blocks;
using pst.impl.decoders.ndb.blocks.data;
using pst.impl.decoders.ndb.blocks.subnode;
using pst.impl.decoders.ndb.btree;
using pst.impl.decoders.primitives;
using pst.impl.ltp.bth;
using pst.impl.ltp.hn;
using pst.impl.ltp.pc;
using pst.impl.ltp.tc;
using pst.impl.ndb;
using pst.impl.ndb.bbt;
using pst.impl.ndb.datatree;
using pst.impl.ndb.nbt;
using pst.impl.ndb.subnodebtree;
using pst.interfaces;
using pst.interfaces.btree;
using pst.interfaces.ltp.bth;
using pst.interfaces.ltp.hn;
using pst.interfaces.ltp.pc;
using pst.interfaces.ltp.tc;
using pst.interfaces.ndb;
using pst.utilities;

namespace pst
{
    class PSTServices
    {
        public static readonly IBTreeLeafKeysEnumerator<LNBTEntry, BREF> NodeBTreeKeysEnumerator;
        public static readonly IBTreeLeafKeysEnumerator<LBBTEntry, BREF> BlockBTreeKeysEnumerator;
        public static readonly IBTreeLeafKeysEnumeratorThatDoesntKnowHowToMapTheKeyToNodeReference<SLEntry, SIEntry, LBBTEntry> SubnodeBTreeKeysEnumerator;
        public static readonly IDataTreeLeafNodesEnumerator DataTreeLeafNodesEnumerator;
        public static readonly IPropertiesFromPropertyContextLoader PropertiesFromPropertyContextLoader;
        public static readonly IDecoder<Header> HeaderDecoder;
        public static readonly ITableContextLoader TableContextLoader;

        static PSTServices()
        {
            NodeBTreeKeysEnumerator =
                CreateNodeBTreeLeafKeysTraverser();

            BlockBTreeKeysEnumerator =
                CreateBlockBTreeLeafKeysTraverser();

            DataTreeLeafNodesEnumerator =
                CreateDataTreeLeafNodesEnumerator();

            SubnodeBTreeKeysEnumerator =
                CreateSubnodeBTreeLeafKeysEnumerator();

            PropertiesFromPropertyContextLoader =
                CreatePropertiesFromPropertyContextLoader();

            TableContextLoader =
                CreateTableContextLoader();

            HeaderDecoder =
                new HeaderDecoder(
                    new Int32Decoder(),
                    new RootDecoder(
                        new Int32Decoder(),
                        new Int64Decoder(),
                        new BREFDecoder(
                             new BIDDecoder(),
                             new IBDecoder())),
                    new BIDDecoder(),
                    new NIDDecoder(
                        new Int32Decoder()));
        }

        private static ITableContextLoader CreateTableContextLoader()
        {
            return
                new TableContextLoader(
                    new DataRecordToTCROWIDConverter(
                        new NIDDecoder(
                            new Int32Decoder()),
                        new Int32Decoder()),
                    CreateBTreeOnHeapLeafKeyEnumerator(),
                    CreateHeapOnNodeLoader(),
                    new TCINFODecoder(
                        new Int32Decoder(),
                        new HIDDecoder(
                            new Int32Decoder()),
                        new TCOLDESCDecoder(
                            new Int32Decoder())),
                    new HNIDDecoder(
                        new HIDDecoder(
                            new Int32Decoder()),
                        new NIDDecoder(
                            new Int32Decoder())));
        }

        private static IPropertiesFromPropertyContextLoader CreatePropertiesFromPropertyContextLoader()
        {
            return
                new PropertiesFromPropertyContextLoader(
                    CreateHeapOnNodeLoader(),
                    new HIDDecoder(
                        new Int32Decoder()),
                    new Int32Decoder(),
                    new PropertyTypeDecoder(
                        new Int32Decoder()),
                    new PropertyTypeMetadataProvider(),
                    CreateBTreeOnHeapLeafKeyEnumerator());
        }

        private static IBTreeOnHeapLeafKeysEnumerator CreateBTreeOnHeapLeafKeyEnumerator()
        {
            return
                new BTreeOnHeapLeafKeysEnumerator(
                    CreateHeapOnNodeLoader(),
                    new BTHHEADERDecoder(
                        new Int32Decoder(),
                        new HIDDecoder(
                            new Int32Decoder())),
                    new HIDDecoder(
                        new Int32Decoder()));
        }

        private static IHeapOnNodeLoader CreateHeapOnNodeLoader()
        {
            return new HeapOnNodeLoader(
                    new HNHDRDecoder(
                        new Int32Decoder(),
                        new HIDDecoder(
                            new Int32Decoder())),
                    new HNPAGEHDRDecoder(
                        new Int32Decoder()),
                    new HNPAGEMAPDecoder(
                        new Int32Decoder()),
                    new HNBITMAPHDRDecoder(
                        new Int32Decoder()),
                    new HeapOnNodeItemsLoader(
                        new Int32Decoder()),
                    new DataTreeLeafNodesEnumerator(
                        new BTreeLeafKeyEnumeratorThatDoesntKnowHowToMapKeyToNodeReference<InternalDataBlock, LBBTEntry, BID, BID>(
                            new BIDsFromInternalDataBlockExtractor(
                                new BIDDecoder()),
                            new BIDsFromInternalDataBlockExtractor(
                                new BIDDecoder()),
                            new NodeLevelFromInternalDataBlockExtractor(),
                            new InternalDataBlockLoader(
                                new InternalDataBlockDecoder(
                                    new Int32Decoder(),
                                    new BlockTrailerDecoder(
                                        new BIDDecoder(),
                                        new Int32Decoder())))),
                        new ExternalDataBlockDecoder(
                            new BlockTrailerDecoder(
                                new BIDDecoder(),
                                new Int32Decoder()),
                            new PermutativeDecoder(false))));
        }

        private static IDataTreeLeafNodesEnumerator CreateDataTreeLeafNodesEnumerator()
        {
            return
                new DataTreeLeafNodesEnumerator(
                    new BTreeLeafKeyEnumeratorThatDoesntKnowHowToMapKeyToNodeReference<InternalDataBlock, LBBTEntry, BID, BID>(
                        new BIDsFromInternalDataBlockExtractor(
                            new BIDDecoder()),
                        new BIDsFromInternalDataBlockExtractor(
                            new BIDDecoder()),
                        new NodeLevelFromInternalDataBlockExtractor(),
                        new InternalDataBlockLoader(
                            new InternalDataBlockDecoder(
                                new Int32Decoder(),
                                new BlockTrailerDecoder(
                                    new BIDDecoder(),
                                    new Int32Decoder())))),
                    new ExternalDataBlockDecoder(
                        new BlockTrailerDecoder(
                            new BIDDecoder(),
                            new Int32Decoder()),
                        new PermutativeDecoder(false)));
        }

        private static IBTreeLeafKeysEnumeratorThatDoesntKnowHowToMapTheKeyToNodeReference<SLEntry, SIEntry, LBBTEntry> CreateSubnodeBTreeLeafKeysEnumerator()
        {
            return
                new BTreeLeafKeyEnumeratorThatDoesntKnowHowToMapKeyToNodeReference<SubnodeBlock, LBBTEntry, SIEntry, SLEntry>(
                    new SIEntriesFromSubnodeBlockExtractor(
                        new SIEntryDecoder(
                            new NIDDecoder(
                                new Int32Decoder()),
                            new BIDDecoder())),
                    new SLEntriesFromSubnodeBlockExtractor(
                        new SLEntryDecoder(
                            new NIDDecoder(
                                new Int32Decoder()),
                            new BIDDecoder())),
                    new NodeLevelFromSubnodeBlockExtractor(),
                    new SubnodeBlockLoader(
                        new SubnodeBlockDecoder(
                            new Int32Decoder(),
                            new BlockTrailerDecoder(
                                new BIDDecoder(),
                                new Int32Decoder()))));
        }

        private static IBTreeLeafKeysEnumerator<LNBTEntry, BREF> CreateNodeBTreeLeafKeysTraverser()
        {
            return
                new BTreeLeafKeysEnumerator<BTPage, BREF, INBTEntry, LNBTEntry>(
                    new BREFFromINBTEntryExtractor(),
                    new INBTEntriesFromBTPageExtractor(
                        new INBTEntryDecoder(
                            new NIDDecoder(
                                new Int32Decoder()),
                            new BREFDecoder(
                                new BIDDecoder(),
                                new IBDecoder()))),
                    new LNBTEntriesFromBTPageExtractor(
                        new LNBTEntryDecoder(
                            new NIDDecoder(
                                new Int32Decoder()),
                            new BIDDecoder())),
                    new PageLevelFromBTPageExtractor(),
                    new BTPageLoader(
                        new BTPageDecoder(
                            new Int32Decoder(),
                            new PageTrailerDecoder(
                                new Int32Decoder(),
                                new BIDDecoder()))));
        }

        private static IBTreeLeafKeysEnumerator<LBBTEntry, BREF> CreateBlockBTreeLeafKeysTraverser()
        {
            return
                new BTreeLeafKeysEnumerator<BTPage, BREF, IBBTEntry, LBBTEntry>(
                    new BREFFromIBBTEntryExtractor(),
                    new IBBTEntriesFromBTPageExtractor(
                        new IBBTEntryDecoder(
                            new BIDDecoder(),
                            new BREFDecoder(
                                new BIDDecoder(),
                                new IBDecoder()))),
                    new LBBTEntriesFromBTPageExtractor(
                        new LBBTEntryDecoder(
                            new BREFDecoder(
                                new BIDDecoder(),
                                new IBDecoder()),
                            new Int32Decoder())),
                    new PageLevelFromBTPageExtractor(),
                    new BTPageLoader(
                        new BTPageDecoder(
                            new Int32Decoder(),
                            new PageTrailerDecoder(
                                new Int32Decoder(),
                                new BIDDecoder()))));
        }
    }
}
