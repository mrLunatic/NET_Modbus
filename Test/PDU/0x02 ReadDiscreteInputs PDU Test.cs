using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus;
using Modbus.Common;

namespace Test
{
    [TestClass]
    public class ReadDiscreteInputsPDU_Test
    {
        #region Ranges
        static int MinAddress  = 0x0000;
        static int NormAddress = 0x000A;
        static int MaxAddress  = 0xFFFF;

        static int MinQuantity  = 0x0001;
        static int NormQuantity = 0x000F;
        static int MaxQuantity  = 0x07D0;
        #endregion

        [TestMethod]
        public void ReadDiscreteInputsPDU_ArgumentRange()
        {
            try
            {
                var TestingPDU = new ReadDiscreteInputsPDU(MinAddress - 1, NormQuantity);
                Assert.Fail("Min Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                var TestingPDU = new ReadDiscreteInputsPDU(MaxAddress + 1, NormQuantity);
                Assert.Fail("Max Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                var TestingPDU = new ReadDiscreteInputsPDU(NormAddress, MinQuantity - 1);
                Assert.Fail("Min Quantity Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                var TestingPDU = new ReadDiscreteInputsPDU(NormAddress, MaxQuantity + 1);
                Assert.Fail("Max Quantity Error");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]        // Oficial Example 
        public void ReadDiscreteInputsPDU_DocSampleReq()
        {
            var PDU = new ReadDiscreteInputsPDU(196, 22);

            var ExpectedReq = new byte[] 
            {          
                0x02,       // Function Code
                0x00,       // Starting Address Hi
                0xC4,       // Starting Address Lo
                0x00,       // Quantity of Inputs Hi
                0x16,       // Quantity of Inputs Lo
            };

            var ActualReq = PDU.BuildRequest();

            CollectionAssert.AreEqual(ExpectedReq, ActualReq, "Wrong Request");
        }

        [TestMethod]        // Handling Request on Server
        public void ReadDiscreteInputsPDU_LifeCycle()
        {
            var RandomGen = new Random();

            int Address = RandomGen.Next(MinAddress, MaxAddress);
            int Quantity = RandomGen.Next(MinQuantity, MaxQuantity);

            #region Build Server and Values
            var Server = new BaseModbusServer();
            var Values = new bool[Quantity];

            for (int i = 0; i < Quantity; i++)
            {
                Values[i] = (RandomGen.Next(2) != 0);
                Server.AddDiscreteInput(Address + i, Values[i]);
            }
            #endregion

            var ClientPDU = new ReadDiscreteInputsPDU(Address, Quantity);
            var Request = ClientPDU.BuildRequest();

            var ServerPDU = new ReadDiscreteInputsPDU();
            var ReqStatus = ServerPDU.CheckRequest(Request);

            Assert.AreEqual(Status.OK, ReqStatus, "Recognition Error");
            Assert.AreEqual(Address, ServerPDU.StartingAddress, "Whong Address recognition in CheckRequest");
            Assert.AreEqual(Quantity, ServerPDU.Quantity, "Whong Quantity recognition in CheckRequest");

            ServerPDU.Handle(Server);

            CollectionAssert.AreEqual(Values, ServerPDU.Value, "Whong Address recognition in CheckRequest");

            ClientPDU.CheckResponse(ServerPDU.BuildResponse());

            CollectionAssert.AreEqual(Values, ClientPDU.Value, "Whong Address recognition in CheckRequest");
        }

    }
}
