using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.RTU;

namespace Test.RTU
{
    [TestClass]
    public class CRC_Test
    {
        [TestMethod]
        public void CRCTest()
        {
            var SampleReq = new byte[]
            {
                0x01,
                0x0F,
                0x00,
                0x13,
                0x00,
                0x0A,
                0x02,
                0xCD,
                0x01,
                0x72,   // CRC Lo
                0xCB,   // CRC Hi
            };

            ushort ExpectedCRC = 0xCB72;

            Assert.AreEqual(ExpectedCRC, CRC.Get(SampleReq, 9));
            Assert.AreEqual(0, CRC.Get(SampleReq));
        }
    }
}
