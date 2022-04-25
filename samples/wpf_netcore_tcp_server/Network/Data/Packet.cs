using System;
using Newtonsoft.Json;
using SettingNetwork.Util;

namespace SettingNetwork
{
    [Serializable]
    public partial class Packet : BindableBase
    {
        private string _time;
        private E_ICD_PACKET_TYPE packetType;
        private int from;
        private int to;
        private string _description;

        public string Time { get => _time; set => SetProperty(ref _time, value); }
        public E_ICD_PACKET_TYPE PacketType { get => packetType; set => SetProperty(ref packetType, value); }
        public int From { get => from; set => SetProperty(ref from, value); }
        public int To { get => to; set => SetProperty(ref to, value); }

        [JsonIgnore]
        public string Description
        {
            get
            {
                Description = JsonConvert.SerializeObject(this, Formatting.Indented);
                return _description;
            }
            set => SetProperty(ref _description, value);
        }


        public Packet()
        {
            PacketType = GetPacketType(GetType());
            Time = DateTime.UtcNow.AddHours(9).ToShortDateString() + " " + DateTime.UtcNow.AddHours(9).ToLongTimeString();
        }

        public static Type GetClassType(E_ICD_PACKET_TYPE packetType)
        {
            switch (packetType)
            {
                default:
                case E_ICD_PACKET_TYPE.PacketType_DEFAULT:
                    return typeof(Packet);
                case E_ICD_PACKET_TYPE.PacketType_COMMAND_CONTROL:
                    return typeof(Command_Control);
                case E_ICD_PACKET_TYPE.PacketType_SCENARIO_INFO:
                    return typeof(Scenario_Info);
                case E_ICD_PACKET_TYPE.PacketType_STUDENT_DATA:
                    return typeof(Student_Data);
                case E_ICD_PACKET_TYPE.PacketType_HARDWARE_STATUS:
                    return typeof(Hardware_Status);
                case E_ICD_PACKET_TYPE.PacketType_INSTRUCTOR_MESSAGE:
                    return typeof(Instructor_Message);
            }
        }

        public static E_ICD_PACKET_TYPE GetPacketType(Type classType)
        {
            if (classType == typeof(Command_Control))
            {
                return E_ICD_PACKET_TYPE.PacketType_COMMAND_CONTROL;
            }
            else if (classType == typeof(Scenario_Info))
            {
                return E_ICD_PACKET_TYPE.PacketType_SCENARIO_INFO;
            }
            else if (classType == typeof(Student_Data))
            {
                return E_ICD_PACKET_TYPE.PacketType_STUDENT_DATA;
            }
            else if (classType == typeof(Hardware_Status))
            {
                return E_ICD_PACKET_TYPE.PacketType_HARDWARE_STATUS;
            }
            else if (classType == typeof(Instructor_Message))
            {
                return E_ICD_PACKET_TYPE.PacketType_INSTRUCTOR_MESSAGE;
            }


            return E_ICD_PACKET_TYPE.PacketType_DEFAULT;
        }

        public static E_ICD_PACKET_TYPE GetPacketType<T>(T packet) where T : Packet
        {
            return GetPacketType(typeof(T));
        }

        public E_ICD_PACKET_TYPE GetPacketType()
        {
            return PacketType;
        }

        public Type GetClassType()
        {
            return GetClassType(PacketType);
        }
    }
}
