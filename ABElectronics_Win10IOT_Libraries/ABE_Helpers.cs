using Windows.Devices.I2c;
using System;

namespace ABElectronics_Win10IOT_Libraries
{
    class ABE_Helpers
    {
        internal string I2C_CONTROLLER_NAME = "I2C1";

        /// <summary>
        /// Updates the value of a single bit within a byte and returns the updated byte
        /// </summary>
        /// <param name="value">The byte to update</param>
        /// <param name="position">Position of the bit to change</param>
        /// <param name="bitstate">The new bit value</param>
        /// <returns></returns>
        internal byte UpdateByte(byte value, byte position, bool bitstate)
        {
            if (bitstate)
            {
                //left-shift 1, then bitwise OR
                return (byte)(value | (1 << position));
            }
            else
            {
                //left-shift 1, then take complement, then bitwise AND
                return (byte)(value & ~(1 << position));
            }
        }

        internal int UpdateInt(int value, byte position, bool bitstate)
        {
            if (bitstate)
            {
                //left-shift 1, then bitwise OR
                return value | (1 << position);
            }
            else
            {
                //left-shift 1, then take complement, then bitwise AND
                return value & ~(1 << position);
            }
        }

        /// <summary>
        /// Checks the value of a single bit within a byte
        /// </summary>
        /// <param name="value">The value to query</param>
        /// <param name="position">The bit position within the byte</param>
        /// <returns></returns>
        internal bool CheckBit(byte value, byte position)
        {
            // internal method for reading the value of a single bit within a byte
            return (value & (1 << position)) != 0;
        }

        internal bool CheckIntBit(int value, byte position)
        {
            // internal method for reading the value of a single bit within a byte
            return (value & (1 << position)) != 0;
        }



        /// <summary>
        /// Writes a single byte to an I2C device.
        /// </summary>
        /// <param name="bus">I2C device</param>
        /// <param name="register">Address register</param>
        /// <param name="value">Value to write to the register</param>
        internal void WriteI2CByte(I2cDevice bus, byte register, byte value)
        {
            byte[] writeBuffer = new byte[] { register, value };
            try {                
                bus.Write(writeBuffer);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void WriteI2CSingleByte(I2cDevice bus, byte value)
        {
            byte[] writeBuffer = new byte[] { value };
            try
            {
                bus.Write(writeBuffer);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Read a single byte from an I2C device
        /// </summary>
        /// <param name="bus">I2C device</param>
        /// <param name="register">Address register to read from</param>
        /// <returns></returns>
        internal byte ReadI2CByte(I2cDevice bus, byte register)
        {
            try
            {
                byte[] readBuffer = new byte[] { register };
                byte[] returnValue = new byte[1];

            bus.WriteRead(readBuffer, returnValue);

            return returnValue[0];
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal byte[] ReadI2CBlockData(I2cDevice bus, byte register, byte bytesToReturn)
        {
            try
            {
                byte[] readBuffer = new byte[] { register };
                byte[] returnValue = new byte[bytesToReturn];

                bus.WriteRead(readBuffer, returnValue);

                return returnValue;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
