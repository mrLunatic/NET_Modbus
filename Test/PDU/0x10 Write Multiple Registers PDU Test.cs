using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus;
using Modbus.Common;

namespace Test
{
    [TestClass]
    public class WriteMultipleRegistersPDU_Test
    {
        #region Ranges
        static int MinAddress = 0x0000;
        static int NormAddress = 0x000A;
        static int MaxAddress = 0xFFFF;

        static int MinQuantity = 0x0001;
        static int NormQuantity = 0x000F;
        static int MaxQuantity = 0x007B;
        #endregion

        [TestMethod]
        public void WriteMultipleRegistersPDU_ArgumentRange()
        {
            IProtocolDataUnit TestingPDU;

            try
            {
                TestingPDU = new WriteMultipleRegistersPDU(MinAddress - 1, new int[NormQuantity]);
                Assert.Fail("Min Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteMultipleRegistersPDU(MaxAddress + 1, new int[NormQuantity]);
                Assert.Fail("Max Address Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteMultipleRegistersPDU(NormAddress, new int[MinQuantity - 1]);
                Assert.Fail("Min Quantity Error");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                TestingPDU = new WriteMultipleRegistersPDU(NormAddress, new int[MaxQuantity + 1]);
                Assert.Fail("Max Quantity Error");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]        // Oficial Example 
        public void WriteMultipleRegistersPDU_DocSampleReq()
        {
            var ClientPDU = new WriteMultipleRegistersPDU(1, new int[] { 10, 258 });

            var DacRequest = new byte[] 
            {          
                0x10,       // Code Function
                0x00, 0x01, // Starting Address
                0x00, 0x02, // Quantity
                0x04,       // Byte Count
                0x00, 0x0A, // Value[0]
                0x01, 0x02, // Value[1]
            };

            var OKResponse = new byte[]
            {
                0x10,       // Code Function
                0x00, 0x01, // Starting Address
                0x00, 0x02, // Quantity
            };

            var ExResponse = new byte[]
            {
                0x90,
                0x01,
            };


            CollectionAssert.AreEqual(DacRequest, ClientPDU.BuildRequest(), "Wrong Request");
            Assert.AreEqual(Status.OK, ClientPDU.CheckResponse(OKResponse), "OK Status not recognited");
            Assert.AreEqual(Status.Exception, ClientPDU.CheckResponse(ExResponse), "Exception Status not recognited");
            Assert.AreEqual(0x01, ClientPDU.ExceptionCode, "Exception not recognited");
        }

        [TestMethod]        // Handling Request on Server
        public void WriteMultipleRegistersPDU_LifeCycle()
        {
            var RandomGen = new Random();

            int Address = RandomGen.Next(MinAddress, MaxAddress);
            int Quantity = RandomGen.Next(MinQuantity, MaxQuantity);

            #region Build Server and Values
            var Server = new BaseModbusServer();
            var Value = new int[Quantity];

            for (int i = 0; i < Quantity; i++)
            {
                Value[i] = RandomGen.Next(0xFFFF);
                Server.AddHoldingRegister(Address + i);
            }
            #endregion

            var ClientPDU = new WriteMultipleRegistersPDU(Address, Value);
            var Request = ClientPDU.BuildRequest();

            var ServerPDU = new WriteMultipleRegistersPDU();

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
