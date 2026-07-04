using Elasticsearch.Net;

namespace Example.Common.Const
{
    public class KafkaConst
    {
        public const string HeaderAction = "action";
        public const string HeaderDeviceId = "deviceId";
        public const string HeaderCustomerId = "customerId";
    }

    public class KafkaTopicConst
    {
        // Consumer trên box thì tạo mỗi khách hàng 1 topic
        // Consumer trên platform thì mỗi package 1 topic

        public const string Test = "test";
        public const string HealthCheckCamera = "helthcheckcamera";
        public const string UserCommonAll = "usercommon_customer{0}_all";
        public const string EmployeeCommonAll = "employeecommon_customer{0}_all";
        public const string UserCommonEmployee = "usercommon_customer{0}_device_employee";
        public const string UserCommonEmployeeGroup = "usercommon_customer{0}_employee_group";

        public const string CDCustomerAll = "customer_detection_customer{0}_all";
        public const string CDCustomerGroup = "customer_detection_customer{0}_customer_group";
        public const string CustomerDetectionEvidence = "customer_detection_evidence";
        public const string EmployeeMonitoringEvidence = "employee_monitoring_evidence";
        public const string CDCustomerGroupTopicForApp = "customer_detection_customer{0}_customer_group_{1}";
        public const string FaceWelcomeEvent = "facewelcome_customer{0}_event";
        public const string FaceWelcomeGroupUser = "facewelcome_customer{0}_group_user";
        public const string FaceWelcomeConfig = "facewelcome_customer{0}_config";
        public const string FaceWelcomeEventHistories = "facewelcome_event_histories";

        public const string PeopleCountingEvent = "peoplecounting_customer{0}_event";
        public const string PeopleCountingUniformAll = "uniform_customer{0}_all";

        public const string UniformDetectionAll = "uniform_detection_customer{0}_all";

        public const string TableWorkingSchedule = "working_schedule_customer{0}";

        public const string SecurityMonitoringFence = "security_monitoring_customer{0}_fence";
        public const string SecurityMonitoringCamera = "security_monitoring_customer{0}_camera";
        public const string SecurityMonitoringEvidence = "security_monitoring_evidence";
        public const string SecurityMonitoringCameraIotConfig = "security_monitoring_iot_config_customer{0}_camera";
        public const string SecurityMonitoringIOT = "security_monitoring_customer{0}_iot";
        public const string SecurityMonitoringCallIot = "security_monitoring_call_iot_customer{0}";

        public const string ZoneManagementControlZone = "zone_management_customer{0}_controlzone";
        public const string ZoneManagementEvidence = "zone_management_evidence";

        public const string TrafficControlGate = "traffic_control_customer{0}_gate";
        public const string TrafficControlHistories = "traffic_control_histories";

        public const string LoitererConfig = "loiterer_customer{0}_config";
        public const string LoitererEvidence = "loiterer_evidence";

        public const string WeaponCarrierConfig = "weapon_carrier_customer{0}_config";
        public const string WeaponCarrierEvidence = "weapon_carrier_evidence";

        public const string FaceIDConfig = "faceid_customer{0}_config";
        public const string FaceIDAccessControl = "faceid_customer{0}_access_control";

        public const string FaceIDAccessControlHistories = "faceid_access_control_histories";

        public const string CrowdDetectionConfig = "crowd_detection_customer{0}_config";
        public const string CrowdDetectionEvidence = "crowd_detection_evidence";

        public const string SystemConfigDefault = "system_config_customer0";
        public const string SystemConfig = "system_config_customer{0}";

        public const string WalkingZoneEvidence = "walking_zone_evidence";


        public const string VaultMonitoringEvidence = "vault_monitoring_evidence";
        public const string VaultMonitoringCamera = "vault_monitoring_customer{0}_camera";
        //public const string VaultMonitoringCameraIotConfig = "vault_monitoring_iot_config_customer{0}_camera";
        public const string VaultMonitoringIOT = "vault_monitoring_customer{0}_iot";
        public const string VaultMonitoringAccessControl = "vault_monitoring_customer{0}_access_control";
        public const string VaultMonitoringArea = "vault_monitoring_customer{0}_area";

        public const string SshTunnelAction = "ssh_tunnel_action_customer{0}";
        public const string SshTunnelForward = "ssh_tunnel_forward_customer{0}_deviceid{1}";

        public const string TrafficSummary = "traffic_summaries";
        public const string TrafficDetail = "traffic_details";

        public const string TableCamera = "camera_customer{0}";
        public const string TableZone = "zone_customer{0}";

        public const string ServiceLevelAgreementEvidence = "service_level_agreement_evidence";

        public const string UniformDetectionEvidence = "uniform_detection_evidence";
        public const string EmployeeComplianceEvidence = "employee_compliance_evidence";
        public const string AreaMonitoringEvidence = "area_monitoring_evidence";
        public const string FireSmokeDetectionEvidence = "fire_smoke_detection_evidence";
        public const string BiosafetyComplianceMonitoringEvidence = "biosafety_compliance_monitoring_evidence";
    }
}
