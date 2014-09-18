using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Modbus;
using Modbus.Common;

namespace Modbus.TCP
{
    class TCP_ADU : IApplicationDataUnit
    {
        struct MBAP
        {
            public int TransactionID;
            public int ProtocolID;
            public int Lenght;
            public int UnitID;

            public MBAP(int ID, int ReqLenght)
            {
                var Gen = new Random();

                TransactionID = Gen.Next(0xFFFF);
                ProtocolID = 0;
                Lenght = ReqLenght + 1;
                UnitID = ID;
            }

            public MBAP(byte[] Array)
            {
                TransactionID = ( Array[0]<<8 ) + Array[1];
                ProtocolID = (Array[2] << 8) + Array[3];
                Lenght = (Array[4] << 8) + Array[5];
                UnitID = Array[6];
            }
        }

        public IProtocolDataUnit PDU { get; private set; }
        public IModbusDevice Device { get; private set; }
        MBAP Header { get; set; }

        public int ReqSize
        {
            get { return PDU.ReqSize + 7; }
        }
        public int RespSize
        {
            get { return PDU.RespSize + 7; }
        }

        public TCP_ADU(TCP_Device Device)
        {
            this.Device = Device;
        }
        public TCP_ADU(TCP_Device Device, IProtocolDataUnit PDU)
        {
            this.PDU = PDU;
            this.Device = Device;
        }

        public byte[] BuildRequest()
        {
            var Request = new byte[ReqSize];

            Header = new MBAP(Device.Address, PDU.ReqSize);

            Request[0] = (byte)((Header.TransactionID >> 8) & 0xFF);
            Request[1] = (byte)( Header.TransactionID  & 0xFF);
            Request[2] = (byte)((Header.ProtocolID >> 8) & 0xFF);
            Request[3] = (byte)( Header.ProtocolID  & 0xFF);
            Request[4] = (byte)((Header.Lenght >> 8) & 0xFF);
            Request[5] = (byte)( Header.Lenght & 0xFF);
            Request[6] = (byte)( Header.UnitID );

            var PDUReq = PDU.BuildRequest();

            PDUReq.CopyTo(Request, 7);

            return Request;
        }
        public Status CheckResponse(byte[] Response)
        {
            if (Response.Length < 9)
                return Status.Error;

            if (
                (Header.TransactionID != (Response[0] << 8) + Response[1]) ||
                (Header.ProtocolID != (Response[2] << 8) + Response[3]) ||
                (Header.Lenght != (Response[4] << 8) + Response[5]) ||
                (Header.UnitID != Response[6]))
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
            if  (Request.Length < 13)    
                return Status.Error;

            Header = new MBAP(Request);

            if (Header.ProtocolID != 0)
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


            PDUResp.CopyTo(Response, 1);

   

            return Response;
        }

    }
}
