using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus;
using Modbus.Common;

namespace Test
{
    [TestClass]
    public class WriteMultipleCoilsPDU_Test
    {
        #region Ranges
        static int MinAddress = 0x0000;
        static int NormAddress = 0x000A;
        static int MaxAddress = 0xFFFF;

        static int MinQuantity = 0x0001;
        static int NormQuantity = 0x000F;
        static int MaxQuantity = 0x07B0;
        #endregion

        [TestMethod]
        public void WriteMultipleCoilsPDU_ArgumentRange()
        {
            IProtocolDataUnit TestingPDU;

            try
            {
                TestingPDU = new WriteMultipleCoilsPDU(MinAddress - 1, new bool[NormQuantity]);
                Assert.Fail("Min Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteMultipleCoilsPDU(MaxAddress + 1, new bool[NormQuantity]);
                Assert.Fail("Max Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteMultipleCoilsPDU(NormAddress, new bool[MinQuantity - 1]);
                Assert.Fail("Min Quantity Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteMultipleCoilsPDU(NormAddress, new bool[MaxQuantity + 1]);
                Assert.Fail("Max Quantity Error");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]        // Oficial Example 
        public void WriteMultipleCoilsPDU_DocSampleReq()
        {
            var PDU = new WriteMultipleCoilsPDU(19, new bool[] { true, false, true, true, false, false, true, true, true, false });

            var ExpectedReq = new byte[] 
            {          
                0x0F,       // Function Code
                0x00,       // Starting Address Hi
                0x13,       // Starting Address Lo
                0x00,       // Quantity of Inputs Hi
                0x0A,       // Quantity of Inputs Lo
                0x02,       // Byte Count
                0xCD,       // Output Value Hi
                0x01,       // Output Value Lo
            };

            var ActualReq = PDU.BuildRequest();

            CollectionAssert.AreEqual(ExpectedReq, ActualReq, "Wrong Request");
        }

        [TestMethod]        // Handling Request on Server
        public void WriteMultipleCoilsPDU_LifeCycle()
        {
            var RandomGen = new Random();

            int Address = RandomGen.Next(MinAddress, MaxAddress);
            int Quantity = RandomGen.Next(MinQuantity, MaxQuantity);

            #region Build Server and Values
            var Server = new BaseModbusServer();
            var Value = new bool[Quantity];

            for (int i = 0; i < Quantity; i++)
            {
                Value[i] = (RandomGen.Next(2) != 0);
                Server.AddCoil(Address + i);
            }
            #endregion

            var ClientPDU = new WriteMultipleCoilsPDU(Address, Value);
            var Request = ClientPDU.BuildRequest();

            var ServerPDU = new WriteMultipleCoilsPDU();
            var ReqStatus = ServerPDU.CheckRequest(Request);

            Assert.AreEqual(Status.OK, ReqStatus, "Recognition Error");
            Assert.AreEqual(Address, ServerPDU.StartingAddress, "Whong Address recognition in CheckRequest");

            ServerPDU.Handle(Server);

            CollectionAssert.AreEqual(Value, ServerPDU.Value, "Whong Address recognition in CheckRequest");

            ClientPDU.CheckResponse(ServerPDU.BuildResponse());

            CollectionAssert.AreEqual(Value, ClientPDU.Value, "Whong Address recognition in CheckRequest");
        }


    }
}
