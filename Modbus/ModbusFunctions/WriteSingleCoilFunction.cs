using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    public class WriteSingleCoilFunction : ModbusFunction
    {
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters mcp = CommandParameters as ModbusWriteCommandParameters;
            byte[] mdbRequest = new byte[12];

            mdbRequest[0] = (byte)(mcp.TransactionId >> 8);
            mdbRequest[1] = (byte)(mcp.TransactionId & 0x00FF);
            mdbRequest[2] = 0;
            mdbRequest[3] = 0;
            mdbRequest[4] = 0;
            mdbRequest[5] = 6;
            mdbRequest[6] = mcp.UnitId;
            mdbRequest[7] = mcp.FunctionCode;
            mdbRequest[8] = (byte)(mcp.OutputAddress >> 8);
            mdbRequest[9] = (byte)(mcp.OutputAddress & 0x00FF);
            // Coil: 0xFF00 = ON, 0x0000 = OFF
            ushort coilValue = (mcp.Value == 0) ? (ushort)0x0000 : (ushort)0xFF00;
            mdbRequest[10] = (byte)(coilValue >> 8);
            mdbRequest[11] = (byte)(coilValue & 0x00FF);

            return mdbRequest;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters mcp = CommandParameters as ModbusWriteCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == mcp.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            ushort outputAddress = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 8));
            ushort rawValue = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 10));
            ushort coilState = (rawValue == 0xFF00) ? (ushort)1 : (ushort)0;
            result.Add(Tuple.Create(PointType.DIGITAL_OUTPUT, outputAddress), coilState);

            return result;
        }
    }
}