using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters mcp = CommandParameters as ModbusReadCommandParameters;
            byte[] mdbRequest = new byte[12];

            // Transaction ID (big endian)
            mdbRequest[0] = (byte)(mcp.TransactionId >> 8);
            mdbRequest[1] = (byte)(mcp.TransactionId & 0x00FF);
            // Protocol ID = 0
            mdbRequest[2] = 0;
            mdbRequest[3] = 0;
            // Length = 6 (UnitId + FunctionCode + StartAddress[2] + Quantity[2])
            mdbRequest[4] = 0;
            mdbRequest[5] = 6;
            // Unit ID
            mdbRequest[6] = mcp.UnitId;
            // Function Code
            mdbRequest[7] = mcp.FunctionCode;
            // Start Address (big endian)
            mdbRequest[8] = (byte)(mcp.StartAddress >> 8);
            mdbRequest[9] = (byte)(mcp.StartAddress & 0x00FF);
            // Quantity (big endian)
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

            // Byte 8 = ByteCount, data starts at byte 9
            // Coils are packed: bit 0 of first byte = first coil (LSB first)
            for (int i = 0; i < mcp.Quantity; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                ushort value = (ushort)((response[9 + byteIndex] >> bitIndex) & 0x01);
                result.Add(Tuple.Create(PointType.DIGITAL_OUTPUT, (ushort)(mcp.StartAddress + i)), value);
            }

            return result;
        }
    }
}