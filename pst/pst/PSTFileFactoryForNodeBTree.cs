﻿using pst.encodables.ndb;
using pst.encodables.ndb.blocks.subnode;
using pst.encodables.ndb.btree;
using pst.impl;
using pst.impl.btree;
using pst.impl.decoders.ndb;
using pst.impl.decoders.ndb.btree;
using pst.impl.io;
using pst.impl.ndb;
using pst.impl.ndb.nbt;
using pst.interfaces;
using pst.interfaces.btree;
using pst.interfaces.ndb;
using System.IO;

namespace pst
{
    public partial class PSTFile
    {
        private static INodeEntryFinder CreateNodeEntryFinder(
            Stream dataStream,
            ICache<BID, BTPage> cachedBTPages,
            ICache<BID, SubnodeBlock> cachedSubnodeBlocks,
            IDataHolder<Header> cachedHeaderHolder)
        {
            return
                new NodeEntryFinder(
                    CreateHeaderUsageProvider(dataStream, cachedHeaderHolder), 
                    CreateNodeBTreeEntryFinder(dataStream, cachedBTPages), 
                    CreateSubNodesEnumerator(dataStream, cachedBTPages, cachedSubnodeBlocks, cachedHeaderHolder));
        }

        private static IBTreeEntryFinder<NID, LNBTEntry, BREF> CreateNodeBTreeEntryFinder(
            Stream dataStream,
            ICache<BID, BTPage> cachedBTPages)
        {
            return
                new BTreeEntryFinder<NID, LNBTEntry, INBTEntry, BREF, BTPage>(
                    new FuncBasedExtractor<LNBTEntry, NID>(
                        entry => entry.NodeId),
                    new FuncBasedExtractor<INBTEntry, NID>(
                        entry => entry.Key),
                    new FuncBasedExtractor<INBTEntry, BREF>(
                        entry => entry.ChildPageBlockReference),
                    new INBTEntriesFromBTPageExtractor(
                        new INBTEntryDecoder(
                            new BREFDecoder(
                                new BIDDecoder(),
                                new IBDecoder()))),
                    new LNBTEntriesFromBTPageExtractor(
                        new LNBTEntryDecoder(
                            new BIDDecoder())),
                    new FuncBasedExtractor<BTPage, int>(
                        page => page.PageLevel),
                    new BTPageLoader(
                        new DataReader(dataStream),
                        new BTPageDecoder(
                            new PageTrailerDecoder(
                                new BIDDecoder())),
                        cachedBTPages));
        }
    }
}
