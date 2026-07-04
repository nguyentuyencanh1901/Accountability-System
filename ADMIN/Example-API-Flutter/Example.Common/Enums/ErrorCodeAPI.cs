using System.ComponentModel;

namespace Example.Common.Enums
{
    public enum ErrorCodeAPI
    {
        #region System Error
        [Description("INTERNAL_ERROR")]
        InternalError = 101,
        [Description("OK")]
        OK = 200,
        [Description("NO_CONTENT")]
        NoContent = 204,
        [Description("REDIRECT")]
        Redirect = 302,
        [Description("BAD_REQUEST")]
        BadRequest = 400,
        [Description("UNAUTHORIZED")]
        Unauthorized = 401,

        [Description("INVALID_PASSWORD")]
        InvalidPassword = 4001,
        [Description("FORBIDDEN")]
        Forbidden = 403,
        [Description("NOT_FOUND")]
        NotFound = 404,
        #endregion

        #region Common Error
        [Description("NOT_OK")]
        NotOk = 501,
        [Description("CAN_NOT_ACTIVE_DEPARTMENT_BECAUSE_IT_GOT_ONE_OR_MORE_PARRENT_DEPARTMENT_INACTIVE")]
        NotActiveDepartment = 502,
        [Description("CAN_NOT_ACTIVE_AREA_BECAUSE_IT_GOT_ONE_OR_MORE_PARRENT_AREA_INACTIVE")]
        NotActiveArea = 503,

        [Description("NO_CONFIG")]
        NoConfig = 507,

        [Description("Error when call API external")]
        ERRORAPI = 508,
        #endregion

        #region User Error
        [Description("USER_ACCOUNT_HAS_BEEN_UNLOCKED")]
        AccountUnlocked = 600,
        [Description("DUPLICATES")]
        Duplicate = 601,
        [Description("YOUR_ACCOUNT_HAS_BEEN_LOCKED")]
        UserBanned = 602,
        [Description("THE_ACCOUNT_HAS_NOT_BEEN_ACTIVATED")]
        AccountUnActive = 603,
        [Description("INCORRECT_ACCOUNT_OR_PASSWORD_INFOMATION")]
        AccountNotFound = 604,
        [Description("WE_HAVE_RECEIVED_A_REQUEST_FOR_YOUR_PASSWORD._TAKE_A_FEW_MINUTES_TO_CHECK_YOUR_EMAIL_AND_FOLLOW_THE_INSTRUCTIONS_TO_CREATE_A_NEW_PASSWORD")]
        ForgotSuccess = 605,
        [Description("USER_ALREADY_EXISTS!")]
        UserExisted = 606,
        [Description("INVALID_ACCESS_TOKEN_OR_REFRESH_TOKEN")]
        InvalidToken = 607,
        [Description("INVALID_CREDENTIALS")]
        InvalidCredentials = 608,
        [Description("YOUR_ACCOUNT_HAS_BEEN_LOCKED")]
        AccountLocked = 609,
        [Description("PHONE_NUMBERS_HAVE_BEEN_USED")]
        ExistedMobile = 610,
        [Description("WE_CANNOT_ACTIVATE_THE_EMAIL_THAT_YOU_PROVIDED_FOR_US")]
        ErrorActiveMail = 611,
        [Description("EMAIL/CELLPHONE_NUMBER_ALREADY_USED")]
        ExistedAccount = 612,
        [Description("CAN_NOT_RETRIEVE_EMAIL_OR_EMAIL_IS_EMPTY")]
        InvalidEmpty = 613,
        [Description("INVALID_INPUT")]
        InvalidInput = 614,
        [Description("ACCOUNT_NOT_VERIFY")]
        AccountNotVerified = 615,
        [Description("CURRENT_PASSWORD_INVALID")]
        CurrentPassInvalid = 616,
        [Description("DUPLICATES_USERNAME")]
        DuplicateUserName = 617,
        [Description("DUPLICATES_EMAIL")]
        DuplicateEmail = 618,
        [Description("USER_NOT_FOUND")]
        UserNotFound = 619,
        [Description("CUSTOMER_NOT_FOUND")]
        CustomerNotFound = 620,
        [Description("CONFIG_NOT_FOUND")]
        ConfigNotFound = 621,
        [Description("DUPLICATE_CITIZEN_IDENTITY_CARD")]
        DuplicateCitizenIdentityCard = 622,
        [Description("DUPLICATE_CODE")]
        DuplicateCode = 623,

        [Description("DATA_HAS_BEEN_USED")]
        DataHasBeenUsed = 624,

        [Description("DATA_EMPLOYEE_GROUP_HAS_BEEN_USED")]
        DataEmployeeGroupHasBeenUsed = 625,
        
        [Description("DATA_CDCUSTOMER_HAS_BEEN_USED")]
        DataCDCustomerGroupHasBeenUsed = 626,
        #endregion

        #region FileError
        [Description("INVALID_FILE_IMPORT")]
        InvalidFile = 700,
        [Description("INVALID_FILE_IMAGE_UPLOAD")]
        InvalidFileImageUpload = 701,
        #endregion

        #region PackageError
        [Description("CAN_NOT_DEACTIVATE_ALL_VERIONS")]
        InvalidCanNotDeactivateAllVersion = 800,
        [Description("CAN_NOT_ACTIVE_PACKAGE_VERSION_BECAUSE_IT_GOT_ONE_OR_MORE_SERVICE_INACTIVE")]
        InvalidVersionDetailGotInactiveServiceVersion = 801,
        [Description("SERVICE_VERSION_DID_NOT_EXISTS_OR_DUPLICATE")]
        InvalidServiceVersionDuplicateOrDoNotExists = 802,
        [Description("SERVICE_VERSION_IS_IN_USE")]
        InvalidServiceVersionIsInUse = 803,
        [Description("ACTIVE_ANY_VERSION_BEFORE_ACTIVE_PACKAGE")]
        InvalidActivePackageWithoutActiveVersion = 804,
        [Description("CAN_NOT_SET_CURRENT_VERSION_WITH_INACTIVE_VERSION")]
        InvalidCanNotSetCurrentVersionWithInactiveVersion = 805,
        [Description("PACKAGE_HAS_USE_BY_CUSTOMER")]
        InvalidPackageHasUseByCustomer = 806,
        [Description("PACKAGE_HAS_INACTIVE")]
        InvalidPackageHasInActive = 807,
        #endregion

        #region Device Error
        [Description("BOX_NOT_FOUND")]
        BoxNotFound = 900,
        [Description("CAMERA_NOT_FOUND")]
        CameraNotFound = 901,
        #endregion

        #region ZoneManagementPackage
        [Description("CAMERA_IN_OTHER_CONTROL_ZONE")]
        CameraInOtherControlZone = 1000,
        [Description("USER_IN_OTHER_WORKSHIFT_TIME")]
        UserInOtherWorkShiftTime = 1001,
        #endregion

        #region GroupFeature
        [Description("CAMERA_IN_GROUP_FEATURE")]
        CameraInGroupFeature = 1002,
        [Description("NAME_IS_EMPTY")]
        NameIsEmpty = 1004,
        [Description("AREA_IS_CHANGED")]
        AreaIsChanged = 1005,
        [Description("DEVICE_IS_CHANGED")]
        DeviceIsChanged = 1006,
        #endregion

        #region PeopleCounting
        [Description("EVENT_OVERLAP_TIME")]
        EventOverlapTime = 1100,
        #endregion

        #region WorkingSchedule
        [Description("WORKING_SCHEDULE_OVERLAP_TIME")]
        WorkingScheduleOverlapTime = 1200,
        #endregion

        #region Uniform
        [Description("DUPLICATE_UNIFORM_CODE")]
        DuplicateUniformCode = 1300,
        [Description("DUPLICATE_UNIFORM_NAME")]
        DuplicateUniformName = 1301,
        #endregion

        #region NotExist
        [Description("DOES_NOT_EXIST")]
        CodeDoesNotExist = 1400,
        #endregion

        #region NotDuplicated
        [Description("DOES_NOT_DUPLICATED")]
        CodeNotDuplicated = 1500,
        #endregion
    }
}
