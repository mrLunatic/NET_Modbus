using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus;
using Modbus.Common;

namespace Test
{
    [TestClass]
    public class WriteSingleRegisterPDU_Test
    {
        #region Ranges
        static int MinAddress = 0x0000;
        static int MaxAddress = 0xFFFF;
        #endregion

        [TestMethod]
        public void WriteSingleRegisterPDU_ArgumentRange()
        {
            IProtocolDataUnit TestingPDU;

            try
            {
                TestingPDU = new WriteSingleRegisterPDU(MinAddress - 1, 5);
                Assert.Fail("Min Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteSingleRegisterPDU(MaxAddress + 1, 5);
                Assert.Fail("Max Address Error");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]        // Oficial Example 
        public void WriteSingleRegisterPDU_DocSampleReq()
        {
            var PDU = new WriteSingleRegisterPDU(1, 3);

            var ExpectedReq = new byte[] 
            {          
                0x06,           // Function Code
                0x00,   0x01,   // Starting Address
                0x00,   0x03,   // Value
            };

            var OkResponse = new byte[]
            {
                0x06,           // Function Code
                0x00,   0x01,   // Starting Address
                0x00,   0x03,   // Value
            };

            var ExResponse = new byte[]
            {
                0x86,
                0x01,
            };

            CollectionAssert.AreEqual(ExpectedReq, PDU.BuildRequest(), "Wrong Request");
            Assert.AreEqual(Status.OK, PDU.CheckResponse(OkResponse), "OK status not recognited");
            Assert.AreEqual(Status.Exception, PDU.CheckResponse(ExResponse), "Exception status not recognited");


        }

        [TestMethod]        // Handling Request on Server
        public void WriteSingleRegisterPDU_LifeCycle()
        {
            var RandomGen = new Random();

            int Address = RandomGen.Next(MinAddress, MaxAddress);
            var Value = RandomGen.Next(0xFFFF);

            var Server = new BaseModbusServer();


            Server.AddHoldingRegister(Address);

            var ClientPDU = new WriteSingleRegisterPDU(Address, Value);
            var Request = ClientPDU.BuildRequest();

            var ServerPDU = new WriteSingleRegisterPDU();
            ServerPDU.CheckRequest(Request);

            Assert.AreEqual(Address, ServerPDU.StartingAddress, "Whong Address recognition in CheckRequest");

            ServerPDU.Handle(Server);
            Assert.AreEqual(Value, ServerPDU.Value, "Whong Address recognition in CheckRequest");

            ClientPDU.CheckResponse(ServerPDU.BuildResponse());
            Assert.AreEqual(Value, ClientPDU.Value, "Whong Address recognition in CheckRequest");
        }


    }
}
