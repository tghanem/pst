﻿using pst.interfaces;
using pst.utilities;
using pst.encodables.ltp;

namespace pst.impl.decoders.ltp
{
    class HIDDecoder : IDecoder<HID>
    {
        private readonly IDecoder<int> int32Decoder;

        public HIDDecoder(IDecoder<int> int32Decoder)
        {
            this.int32Decoder = int32Decoder;
        }

        public HID Decode(BinaryData encodedData)
        {
            using (var parser = BinaryDataParser.OfValue(encodedData))
            {
                var value = parser.TakeAndSkip(4, int32Decoder);

                return
                    new HID(
                        value & 0x0000001F,
                        (value >> 5) & 0x000000FE,
                        (value >> 16) & 0x0000FFFF);
            }
        }
    }
}