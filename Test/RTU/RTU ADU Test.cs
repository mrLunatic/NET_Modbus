using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus;
using Modbus.Common;
using Modbus.RTU;

namespace Test.RTU
{
    [TestClass]
    public class RTUADU_Test
    {
        [TestMethod]
        public void RTU_ADU_DocSampleReq()
        {
            var DocRequest = new byte[]
            {
                0x01,
                0x10, 0x00, 0x01, 0x00, 0x01, 0x02, 0xFF, 0xFF,
                0xA6, 0x31,
            };

            var DocResponse = new byte[]
            {
                0x01,
                0x10, 0x00, 0x01, 0x00, 0x01,
                0x50, 0x09,
            };

            var ClientPDU = new WriteMultipleRegistersPDU(1, new int[] { 0xFFFF });
            var Device = new RTU_Device(1);

            var ClientADU = new RTU_ADU(Device, ClientPDU);

            CollectionAssert.AreEqual(DocRequest, ClientADU.BuildRequest());

            Assert.AreEqual(Status.OK, ClientADU.CheckResponse(DocResponse));

            

        }

        [TestMethod]
        public void RTU_ADU_LifeCycle()
        {
            var ClientPDU = new ReadHoldingRegistersPDU(5, 10);
            var Device = new RTU_Device(1);
            var ClientADU = new RTU_ADU(Device, ClientPDU);

            var ServerADU = new RTU_ADU(Device);

            ServerADU.CheckRequest(ClientADU.BuildRequest());
        }
    }
}
