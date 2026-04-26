using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters mcp = CommandParameters as ModbusReadCommandParameters;
            byte[] mdbRequest = new byte[12];

            mdbRequest[0] = (byte)(mcp.TransactionId >> 8);
            mdbRequest[1] = (byte)(mcp.TransactionId & 0x00FF);
            mdbRequest[2] = 0;
            mdbRequest[3] = 0;
            mdbRequest[4] = 0;
            mdbRequest[5] = 6;
            mdbRequest[6] = mcp.UnitId;
            mdbRequest[7] = mcp.FunctionCode;
            mdbRequest[8] = (byte)(mcp.StartAddress >> 8);
            mdbRequest[9] = (byte)(mcp.StartAddress & 0x00FF);
            mdbRequest[10] = (byte)(mcp.Quantity >> 8);
            mdbRequest[11] = (byte)(mcp.Quantity & 0x00FF);

            return mdbRequest;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters mcp = CommandParameters as ModbusReadCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == mcp.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            // Byte 8 = ByteCount, data starts at byte 9 (each register = 2 bytes, big endian)
            for (int i = 0; i < mcp.Quantity; i++)
            {
                int bytePos = 9 + (i * 2);
                ushort value = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, bytePos));
                result.Add(Tuple.Create(PointType.ANALOG_OUTPUT, (ushort)(mcp.StartAddress + i)), value);
            }

            return result;
        }
    }
}