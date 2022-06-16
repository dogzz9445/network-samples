using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SettingNetwork
{
    public enum E_ICD_PACKET_TYPE : int
    {
        PacketType_DEFAULT = 0,
        PacketType_COMMAND_CONTROL,
        PacketType_SCENARIO_INFO,
        PacketType_STUDENT_DATA,
        PacketType_HARDWARE_STATUS,
        PacketType_INSTRUCTOR_MESSAGE,
        PacketType_EVALUATION_ACTION
    }

    public enum E_COMMAND_CONTROL_TYPE : int
    {
        ControlType_Init,
        ControlType_Start,
        ControlType_Stop,
        ControlType_Shutdown
    }

    public class Student_Data : Packet
    {
        public string Name;          // 이름
        public string InfoA;         // 정보
        public int Number;           // 지정호기
        public string ScenarioName;  // 시나리오 이름

        [JsonIgnore]
        private string state;
        public string Tr_State { get => state; set => state = value; }
    }

    public class Scenario_Info : Packet
    {
        public string ScenarioID;    // 이름
        public string ScenarioName;  // 시나리오 이름
    }

    public class Command_Control : Packet
    {
        //훈련 제어 정보
        public E_COMMAND_CONTROL_TYPE TrnControlCommand;
    }

    public class Hardware_Status : Packet
    {
        // 5초당 1번 & 상태변경시 
        //VR => IOS
        public bool PcPower;            // 프로그렘 상태
        public bool UserPresent;        // 유저가 HMD 사용중인지
        public bool HMDPresent;         // HMD 살아있는지
        public bool TreadmillPresent;   // 트레드밀 살아있는지
        public bool HapticPresent;      // 햅틱 살아있는지

        // 확정되지않음
        public int HMDBatteryLevel;
    }

    public class Instructor_Message : Packet
    {
        public string InstructorMessage;
    }

    public class Evaluation_Action : Packet
    {
        public DateTime InfoTime;  // Windows 시스템 시간
        public float RealTime;     // 시나리오 시작하고 실제 흐르는 시간
        public float ScenarioTime; // 시나리오 시작하고 시뮬레이션되는 시간
        public int ActionID;       // 테이블 액션 아이디
    }
}
