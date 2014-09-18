using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus;
using Modbus.Common;

namespace Test
{
    [TestClass]
    public class WriteSingleCoilPDU_Test
    {
        #region Ranges
        static int MinAddress = 0x0000;
        static int MaxAddress = 0xFFFF;
        #endregion

        [TestMethod]
        public void WriteSingleCoilPDU_ArgumentRange()
        {
            IProtocolDataUnit TestingPDU;

            try
            {
                TestingPDU = new WriteSingleCoilPDU(MinAddress - 1, true);
                Assert.Fail("Min Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteSingleCoilPDU(MaxAddress + 1, true);
                Assert.Fail("Min Address Error");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]        // Oficial Example 
        public void WriteSingleCoilPDU_DocSampleReq()
        {
            var PDU = new WriteSingleCoilPDU(172, true);

            var ExpectedReq = new byte[] 
            {          
                0x05,       // Function Code
                0x00,       // Starting Address Hi
                0xAC,       // Starting Address Lo
                0xFF,       // Quantity of Inputs Hi
                0x00,       // Quantity of Inputs Lo
            };

            var ActualReq = PDU.BuildRequest();

            CollectionAssert.AreEqual(ExpectedReq, ActualReq, "Wrong Request");
        }

        [TestMethod]        // Handling Request on Server
        public void WriteSingleCoilPDU_LifeCycle()
        {
            var RandomGen = new Random();

            int Address = RandomGen.Next(MinAddress, MaxAddress);
            var Value = (RandomGen.Next(2) != 0);

            var Server = new BaseModbusServer();
            

            Server.AddCoil(Address);



            var ClientPDU = new WriteSingleCoilPDU(Address, Value);
            var Request = ClientPDU.BuildRequest();

            var ServerPDU = new WriteSingleCoilPDU();
            var ReqStatus = ServerPDU.CheckRequest(Request);

            Assert.AreEqual(Status.OK, ReqStatus, "Recognition Error");
            Assert.AreEqual(Address, ServerPDU.StartingAddress, "Whong Address recognition in CheckRequest");

            ServerPDU.Handle(Server);

            Assert.AreEqual(Value, ServerPDU.Value, "Whong Address recognition in CheckRequest");

            ClientPDU.CheckResponse(ServerPDU.BuildResponse());

            Assert.AreEqual(Value, ClientPDU.Value, "Whong Address recognition in CheckRequest");
        }


    }
}
