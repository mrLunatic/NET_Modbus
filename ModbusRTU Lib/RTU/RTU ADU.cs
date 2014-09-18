using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus;
using Modbus.Common;
using Modbus.RTU;

namespace Modbus.RTU
{
    public class RTU_ADU : IApplicationDataUnit
    {
        public IProtocolDataUnit PDU { get; private set; }
        public IModbusDevice Device { get; private set; }

        public int ReqSize
        {
            get { return PDU.ReqSize + 3; }
        }
        public int RespSize
        {
            get { return PDU.RespSize + 3; }
        }

        public RTU_ADU(RTU_Device Device)
        {
            this.Device = Device;
        }
        public RTU_ADU(RTU_Device Device, IProtocolDataUnit PDU)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Device type");

            this.PDU = PDU;
            this.Device = Device;
        }

        public byte[] BuildRequest()
        {
            var Request = new byte[ReqSize];

            Request[0] = (byte)(Device as RTU_Device).Address;

            var PDUReq = PDU.BuildRequest();
            PDUReq.CopyTo(Request, 1);

            var crc = CRC.Get(Request, PDUReq.Length + 1 );

            Request[PDUReq.Length + 1] = ((byte)(crc & 0xFF));
            Request[PDUReq.Length + 2] = ((byte)(crc >> 8));

            return Request;
        }
        public Status CheckResponse(byte[] Response)
        {
            if (Response.Length < 5)
                return Status.Error;

            if ( (CRC.Get(Response) != 0) && (Response[0] == Device.Address) )
                return Status.Error;

            if ( (Response.Length == RespSize) || (Response.Length == 5) )
            {
                var PDUResponse = new byte[Response.Length - 3];
                Array.Copy(Response, 1, PDUResponse, 0, PDUResponse.Length);
                return PDU.CheckResponse(PDUResponse);
            }
            else
                return Status.Error;






        }

        public Status CheckRequest(byte[] Request)
        {
            if ( (Request.Length < 4) || (Request[0] != Device.Address) )    // 3 = Address + FuncCode + CRC_Lo + CRC_Hi
                return Status.Error;
           
            if (CRC.Get(Request) != 0)
                return Status.Error;

            switch (Request[1])
            {
                case 0x01: PDU = new ReadCoilsPDU(); break;
                case 0x02: PDU = new ReadDiscreteInputsPDU(); break;
                case 0x03: PDU = new ReadHoldingRegistersPDU(); break;
                case 0x04: PDU = new ReadInputRegistersPDU(); break;
                case 0x05: PDU = new WriteSingleCoilPDU(); break;
                case 0x06: PDU = new WriteSingleRegisterPDU(); break;
                case 0x0F: PDU = new WriteMultipleCoilsPDU(); break;
                case 0x10: PDU = new WriteMultipleRegistersPDU(); break;
                default: return Status.Error;
            }

            var PDURequest = new byte[Request.Length - 3];

            for (int i=0; i<PDURequest.Length; i++)
                PDURequest[i] = Request[i + 1];

            return PDU.CheckRequest(PDURequest);
        }
        public byte[] BuildResponse()
        {
            var PDUResp = PDU.BuildResponse();

            var Response = new byte[PDUResp.Length + 3];

            Response[0] = (byte)(Device as RTU_Device).Address;

            PDUResp.CopyTo(Response, 1);

            var crc = CRC.Get(Response, PDUResp.Length + 1);

            Response[PDUResp.Length + 1] = ((byte)(crc & 0xFF));
            Response[PDUResp.Length + 2] = ((byte)(crc >> 8));

            return Response;
        }
    } 
}
