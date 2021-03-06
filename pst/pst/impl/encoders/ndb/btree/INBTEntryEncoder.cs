﻿using pst.interfaces;
using pst.utilities;
using pst.encodables.ndb;
using pst.encodables.ndb.btree;

namespace pst.impl.encoders.ndb.btree
{
    class INBTEntryEncoder : IEncoder<INBTEntry>
    {
        private readonly IEncoder<NID> nidEncoder;

        private readonly IEncoder<BREF> brefEncoder;

        public INBTEntryEncoder(IEncoder<NID> nidEncoder, IEncoder<BREF> brefEncoder)
        {
            this.nidEncoder = nidEncoder;
            this.brefEncoder = brefEncoder;
        }

        public BinaryData Encode(INBTEntry value)
        {
            var generator = BinaryDataGenerator.New();

            return
                generator
                .Append(value.Key, nidEncoder)
                .Append(value.ChildPageBlockReference, brefEncoder)
                .GetData();
        }
    }
}
