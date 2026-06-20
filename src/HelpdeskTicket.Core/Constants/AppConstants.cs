namespace HelpdeskTicket.Core.Constants;

// ─────────────────────────────────────────────────────────────
//  SessionKeys
//  All session key strings used in HelpdeskTicket.Web.
//  Centralised here to prevent magic strings in controllers.
// ─────────────────────────────────────────────────────────────
public static class SessionKeys
{
    public const string JwtToken = "JWT_TOKEN";
    public const string UserId   = "USER_ID";
    public const string FullName = "USER_FULL_NAME";
    public const string Email    = "USER_EMAIL";
    public const string RoleId   = "USER_ROLE_ID";
    public const string RoleName = "USER_ROLE_NAME";
}

// ─────────────────────────────────────────────────────────────
//  Roles  — mirrors tlkpRole seed data
// ─────────────────────────────────────────────────────────────
public static class Roles
{
    public const string Admin     = "Admin";
    public const string Developer = "Developer";
    public const string EndUser   = "EndUser";

    public const int AdminId     = 1;
    public const int DeveloperId = 2;
    public const int EndUserId   = 3;
}

// ─────────────────────────────────────────────────────────────
//  TicketStatusIds  — mirrors tlkpStatus seed data
// ─────────────────────────────────────────────────────────────
public static class TicketStatusIds
{
    //public const int Open           = 1;
    //public const int Assigned       = 2;
    //public const int InProgress     = 3;
    //public const int Pending        = 4;
    //public const int WaitingForUser = 5;
    //public const int Resolved       = 6;
    //public const int Closed         = 7;
    //public const int Reopened       = 8;
    //public const int Cancelled      = 9;
    public const int Open = 1;
    public const int Assigned = 2;
    public const int InProgress = 3;
    public const int Resolved = 4;
    public const int Closed = 5;
}

// ─────────────────────────────────────────────────────────────
//  TicketPriorityIds — mirrors tlkpPriority seed data
// ─────────────────────────────────────────────────────────────
public static class TicketPriorityIds
{
    public const int Low      = 1;
    public const int Medium   = 2;
    public const int High     = 3;
    public const int Critical = 4;
}

// ─────────────────────────────────────────────────────────────
//  StoredProcedures
//  Repositories reference these constants — never raw strings.
// ─────────────────────────────────────────────────────────────
public static class StoredProcedures
{
    // Auth
    public const string GetUserByEmail = "dbo.spGetUserByEmail";
    public const string RegisterUser   = "dbo.spRegisterUser";

    // Ticket
    public const string GetTicketList    = "dbo.spGetTicketList";
    public const string GetTicketById    = "dbo.spGetTicketById";
    public const string CreateTicket     = "dbo.spCreateTicket";
    public const string UpdateTicket     = "dbo.spUpdateTicket";
    public const string GetAllStatus     = "dbo.spGetAllStatus";
    public const string GetAllPriority   = "dbo.spGetAllPriority";
    public const string GetAllCategory   = "dbo.spGetAllCategory";

    // Comment
    public const string AddComment = "dbo.spAddComment";

    // User
    public const string GetUserList      = "dbo.spGetUserList";
    public const string GetUserById      = "dbo.spGetUserById";
    public const string GetDeveloperList = "dbo.spGetDeveloperList";
    public const string UpdateUser       = "dbo.spUpdateUser";

    // Dashboard
    public const string GetDashboardStats = "dbo.spGetDashboardStats";

    // Audit
    public const string GetAuditByTicketId = "dbo.spGetAuditByTicketId";

    // Category
    public const string SaveCategory = "dbo.spSaveCategory";

    // Forgot Password
    public const string UpdatePassword = "dbo.spUpdatePassword";

    //Reset password
    public const string SavePasswordResetToken = "dbo.spSavePasswordResetToken";
    public const string ResetPasswordByToken = "dbo.spResetPasswordByToken";
    public const string ValidatePasswordResetToken = "dbo.spValidatePasswordResetToken";

    //add multiple attachments
    public const string AddTicketAttachment = "dbo.spAddTicketAttachment";
    public const string DeleteTicketAttachment = "dbo.spDeleteTicketAttachment";
    public const string GetTicketAttachments = "dbo.spGetTicketAttachments";

    //get active admins
    public const string GetActiveAdminEmails = "dbo.spGetActiveAdminEmails";
}
