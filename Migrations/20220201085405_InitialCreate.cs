using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HaloCareCore.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessFunctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    function = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    canRead = table.Column<bool>(type: "bit", nullable: false),
                    canEdit = table.Column<bool>(type: "bit", nullable: false),
                    canCreate = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessFunctions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountAttachmentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAttachmentTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountEmailLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LayoutID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountEmailLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountEmailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    templateId = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountEmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountManagementStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    statusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    SchemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountManagementStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountPlanRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    MedicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    planId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountPlanRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountSchemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Authorizations = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSchemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTextTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    templateId = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTextTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    addressID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    line1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    line2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    line3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    zip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesLine3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesZip = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.addressID);
                });

            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Allergy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifieddate = table.Column<DateTime>(type: "datetime", nullable: true),
                    hasAllergy = table.Column<bool>(type: "bit", nullable: false),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentItemLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assignmentItemID = table.Column<int>(type: "int", nullable: false),
                    followedUp = table.Column<bool>(type: "bit", nullable: false),
                    captured = table.Column<bool>(type: "bit", nullable: false),
                    attached = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentItemLogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentItems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assignmentId = table.Column<int>(type: "int", nullable: false),
                    itemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    documentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    capturedId = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentItems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentItemTasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assignmentItemID = table.Column<int>(type: "int", nullable: false),
                    taskTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    requirementId = table.Column<int>(type: "int", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    closedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    closedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    closed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentItemTasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentItemTaskTypes",
                columns: table => new
                {
                    taskId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    taskDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentItemTaskTypes", x => x.taskId);
                });

            migrationBuilder.CreateTable(
                name: "assignmentLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignmentLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    assignmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    assignmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    postponementReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    postpone = table.Column<bool>(type: "bit", nullable: false),
                    postponeToDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    closedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    closedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pathologyID = table.Column<int>(type: "int", nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    attachmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.assignmentID);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentTypes",
                columns: table => new
                {
                    assignmentType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    assignmentDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentTypes", x => x.assignmentType);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    attachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attachmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    attachmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImportAttachmentSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.attachmentID);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Root = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentTypes",
                columns: table => new
                {
                    attachmentType = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    typeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    assignmentItemType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentTypes", x => x.attachmentType);
                });

            migrationBuilder.CreateTable(
                name: "AuthorisationCodes",
                columns: table => new
                {
                    codeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    authCodeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorisationCodes", x => x.codeId);
                });

            migrationBuilder.CreateTable(
                name: "Authorisations",
                columns: table => new
                {
                    authID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    clientNo = table.Column<int>(type: "int", nullable: false),
                    scriptID = table.Column<int>(type: "int", nullable: false),
                    request = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    entryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    authDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorisations", x => x.authID);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationLetters",
                columns: table => new
                {
                    AuthLetterID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthLinkID = table.Column<int>(type: "int", nullable: false),
                    scriptNo = table.Column<int>(type: "int", nullable: false),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignmentID = table.Column<int>(type: "int", nullable: true),
                    sentVia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sentTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationLetters", x => x.AuthLetterID);
                });

            migrationBuilder.CreateTable(
                name: "AutoNeropathies",
                columns: table => new
                {
                    AutoNeropathyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    autoNeuroSympCheck = table.Column<bool>(type: "bit", nullable: false),
                    autoNeuroSympComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoNeropathies", x => x.AutoNeropathyID);
                });

            migrationBuilder.CreateTable(
                name: "BenefitType",
                columns: table => new
                {
                    benefitType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    benefitTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitType", x => x.benefitType);
                });

            migrationBuilder.CreateTable(
                name: "BulkEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeOptionID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailLayoutHeader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailLayoutFooter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAttachment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Excel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Service = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUploaded = table.Column<bool>(type: "bit", nullable: false),
                    FileRoot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUploadedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    FileUploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarePlanPathologies",
                columns: table => new
                {
                    PathologyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Average_glucose_readings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hypoglycemia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeGlucoseTesting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Glucose_diary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LatestHbA1C = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    viralLoads = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    viralLoadAbnormalities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CD4count = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    otherRelatedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    monitorKidneyFunction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    other = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LatestBloodResultsDiscussed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathologyOutstandingCo_Morbidities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarePlanPathologies", x => x.PathologyID);
                });

            migrationBuilder.CreateTable(
                name: "CaseManagerHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    caseManagerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseManagerHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CaseManagers",
                columns: table => new
                {
                    caseManagerNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    caseManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    caseManagerSurname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    workNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseManagers", x => x.caseManagerNo);
                });

            migrationBuilder.CreateTable(
                name: "ChronicMedication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nappiStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChronicMedication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaimsHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    claimMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claimStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    claimDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimsHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clinical",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    height = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    bmi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    bodyServiceArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    clinicalComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    sentToCl = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinical", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clinicalAdditions",
                columns: table => new
                {
                    clinicalAdditionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hospitalisations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    newConditionDiagnosed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    immunisation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinicalAdditions", x => x.clinicalAdditionID);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalHistoryQuestionaire",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiagnosisDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    smoker = table.Column<bool>(type: "bit", nullable: false),
                    NoCigs = table.Column<int>(type: "int", nullable: true),
                    smokingYears = table.Column<int>(type: "int", nullable: true),
                    drinker = table.Column<bool>(type: "bit", nullable: false),
                    NrDrinks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifieddate = table.Column<DateTime>(type: "datetime", nullable: true),
                    socialComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    socialRecordFollowUp = table.Column<bool>(type: "bit", nullable: false),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalHistoryQuestionaire", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalMessageInstructions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ruleMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalMessageInstructions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ruleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    greater = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gtMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    less = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ltMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ruleType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    male = table.Column<bool>(type: "bit", nullable: false),
                    female = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    pathologyField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hasAssignment = table.Column<bool>(type: "bit", nullable: false),
                    assignmentItem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalRuleTypes",
                columns: table => new
                {
                    ruleType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ruleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalRuleTypes", x => x.ruleType);
                });

            migrationBuilder.CreateTable(
                name: "CoMorbidClinicalRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ruleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    greater = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    less = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    pathologyField = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoMorbidClinicalRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ComorbidConditionExclusions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    mappingCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mappingDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    formularyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ICD10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComorbidConditionExclusions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CoMorbidConditions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    coMorbidId = table.Column<int>(type: "int", nullable: false),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CoMorbidTreatement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    treatementEndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    diagnosisDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    icd10code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hasComormid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoMorbidConditions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CoMorbidTypeRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ruleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ruleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoMorbidTypeRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CoMorbidTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    icd10 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoMorbidTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cell = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    landLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    workNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    preferredMethodOfContact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ISeriesCell = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesLandLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesWorkNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISeriesEmail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactID);
                });

            migrationBuilder.CreateTable(
                name: "Demographic",
                columns: table => new
                {
                    demographicCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    demographicName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demographic", x => x.demographicCode);
                });

            migrationBuilder.CreateTable(
                name: "Dependants",
                columns: table => new
                {
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    memberID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dependentCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    initials = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fullName_ul = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MyProperty = table.Column<int>(type: "int", nullable: false),
                    idNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    birthDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    demographic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isPrinciple = table.Column<bool>(type: "bit", nullable: false),
                    isSACitizen = table.Column<bool>(type: "bit", nullable: false),
                    languageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dispensingProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    dependentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    originationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icd10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title_UC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    optOut = table.Column<bool>(type: "bit", nullable: false),
                    prevMembershipNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailoptOut = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependants", x => x.DependantID);
                });

            migrationBuilder.CreateTable(
                name: "DependantType",
                columns: table => new
                {
                    dependentTypeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    dependentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependantType", x => x.dependentTypeCode);
                });

            migrationBuilder.CreateTable(
                name: "DependentPrograms",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependentPrograms", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DiabeticDairy",
                columns: table => new
                {
                    dairyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    returnedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    sentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SingedEducationalNote = table.Column<bool>(type: "bit", nullable: false),
                    TelehonicalEducationalNote = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiabeticDairy", x => x.dairyId);
                });

            migrationBuilder.CreateTable(
                name: "DisclaimerQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishQuestion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfrikaansQuestion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisclaimerQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DisclaimerResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Yes = table.Column<bool>(type: "bit", nullable: false),
                    No = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisclaimerResponse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispensingProviders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensingProviders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorInformation",
                columns: table => new
                {
                    DoctorInformationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    doctorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contactID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    addressID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorInformation", x => x.DoctorInformationID);
                });

            migrationBuilder.CreateTable(
                name: "DoctorQuestionnaireResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    diagnosisDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Pregnant = table.Column<bool>(type: "bit", nullable: false),
                    EDD = table.Column<DateTime>(type: "datetime", nullable: true),
                    PartnerStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whoStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenotypingDone = table.Column<bool>(type: "bit", nullable: false),
                    GenotyingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    tbDiagnosed = table.Column<bool>(type: "bit", nullable: false),
                    tbTreatmentStartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    tbTreatmentEndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    PEP = table.Column<bool>(type: "bit", nullable: false),
                    pepDateofExposure = table.Column<DateTime>(type: "datetime", nullable: true),
                    recDrugs = table.Column<bool>(type: "bit", nullable: false),
                    recDrugsLastUsed = table.Column<DateTime>(type: "datetime", nullable: true),
                    depression = table.Column<bool>(type: "bit", nullable: false),
                    depressionComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    amputationComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amputationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amputationCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryDiabetic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryDiabeticCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryVascular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryVascularCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryNeurological = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryNeurologicalCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryLopsCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryLopsComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryDeformityCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryDeformityComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryPerArterialDiseaseCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryPerArterialDiseaseComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryWoundCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryWoundComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    autoNeuroSympCheck = table.Column<bool>(type: "bit", nullable: false),
                    autoNeuroSympComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visionCheck = table.Column<bool>(type: "bit", nullable: false),
                    eyeTreatmentCheck = table.Column<bool>(type: "bit", nullable: false),
                    eyeTreatments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    occupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    occupationCheck = table.Column<bool>(type: "bit", nullable: false),
                    shiftWorker = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shiftWorkCheck = table.Column<bool>(type: "bit", nullable: false),
                    lypohypertrophy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lypohypertrophyCheck = table.Column<bool>(type: "bit", nullable: false),
                    lypohypertrophyDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    glucoseReading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hypoglycaemiaSymptomsCheck = table.Column<bool>(type: "bit", nullable: false),
                    hypoglycaemiaCheck = table.Column<bool>(type: "bit", nullable: false),
                    hypoglycaemiaComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hypoglycaemiaSymptomsComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hypoglycaemiaAssistance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emergencyRoomVisits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    insulinUseCheck = table.Column<bool>(type: "bit", nullable: false),
                    insulinUseComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sulfonylureaUseCheck = table.Column<bool>(type: "bit", nullable: false),
                    sulfonylureaUseComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ckdStageCheck = table.Column<bool>(type: "bit", nullable: false),
                    ckdStageComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    patientAgeCheck = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AllergyID = table.Column<int>(type: "int", nullable: true),
                    SocialHistoryID = table.Column<int>(type: "int", nullable: true),
                    PodiatryID = table.Column<int>(type: "int", nullable: true),
                    NervousSystemID = table.Column<int>(type: "int", nullable: true),
                    AutoNeropathyID = table.Column<int>(type: "int", nullable: true),
                    VisionID = table.Column<int>(type: "int", nullable: true),
                    HypoglycaemiaID = table.Column<int>(type: "int", nullable: true),
                    QuestionnaireOtherID = table.Column<int>(type: "int", nullable: true),
                    ClinicalHistoryID = table.Column<int>(type: "int", nullable: true),
                    NewBornID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorQuestionnaireResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorReferrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    ReferralDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Referral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    referralFrom = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorReferrals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    doctorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    drFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    drLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    idNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    practiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.doctorID);
                });

            migrationBuilder.CreateTable(
                name: "DoctorsInformation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Line1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Line2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Line3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorsInformation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorTypes",
                columns: table => new
                {
                    doctorType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    typeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorTypes", x => x.doctorType);
                });

            migrationBuilder.CreateTable(
                name: "EducationalNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "emailAttachmentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    EmailID = table.Column<int>(type: "int", nullable: false),
                    Sent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emailAttachmentHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutHeight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutWidth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Root = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    emailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    emailTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emailMassage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    effectivedate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.emailId);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    templateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    templateHeading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    templateSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    templateBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    program = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.templateID);
                });

            migrationBuilder.CreateTable(
                name: "EmployerGroups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployerCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployerDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployerGroups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "EmployerMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployerCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployerDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployerMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    queryId = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    enquiryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "EnrolmentStepsMonitor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hasScript = table.Column<bool>(type: "bit", nullable: false),
                    scriptCaptured = table.Column<bool>(type: "bit", nullable: false),
                    hasPathology = table.Column<bool>(type: "bit", nullable: false),
                    pathologyCaptured = table.Column<bool>(type: "bit", nullable: false),
                    hasClinical = table.Column<bool>(type: "bit", nullable: false),
                    clinicalCaptured = table.Column<bool>(type: "bit", nullable: false),
                    hasDemographic = table.Column<bool>(type: "bit", nullable: false),
                    demographicCaptured = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolmentStepsMonitor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForgottenPasswords",
                columns: table => new
                {
                    ForgottenPasswordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    expiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgottenPasswords", x => x.ForgottenPasswordID);
                });

            migrationBuilder.CreateTable(
                name: "Functions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Functions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hba1cRangeHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    targetOutcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hba1c = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    hba1cEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hba1cRangeHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HC_Member_Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Root = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalAidName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalAidCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarrierCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Records = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HC_Member_Claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HivcomorbidRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comorbid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stage = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HivcomorbidRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HIVRiskRatingRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CD4Count = table.Column<bool>(type: "bit", nullable: false),
                    CD4Percentage = table.Column<bool>(type: "bit", nullable: false),
                    viralLoad = table.Column<bool>(type: "bit", nullable: false),
                    ltValue = table.Column<int>(type: "int", nullable: false),
                    gtValue = table.Column<int>(type: "int", nullable: false),
                    stage = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HIVRiskRatingRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HIVStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HIVStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HIVStagingRiskRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TB = table.Column<bool>(type: "bit", nullable: false),
                    cancer = table.Column<bool>(type: "bit", nullable: false),
                    cdl = table.Column<bool>(type: "bit", nullable: false),
                    renal = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HIVStagingRiskRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationAuthEvents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    refNo = table.Column<int>(type: "int", nullable: false),
                    epiAuth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    eventCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    associatedDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    procedureCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    procedureModifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    main = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    minimunBenefitCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pmbApplicable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cdlCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createddate = table.Column<DateTime>(type: "datetime", nullable: false),
                    additionalDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    finalDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationAuthEvents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationAuths",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    membershipNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    approvalNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    authStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requestingVendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requestingProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    provider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    plannedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    bedDays = table.Column<int>(type: "int", nullable: false),
                    dischargeDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    procedureDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    authAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    authPendingDenyReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendorReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    actualAdminDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    actualDischargeDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    emergency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    epiAuth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    caseClosed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    closedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    authType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    closedReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    clientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    benefitPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    medicalAid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hasBeenHospitalised = table.Column<bool>(type: "bit", nullable: false),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    questionnaireID_old = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationAuths", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationClaimEvents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    refNo = table.Column<int>(type: "int", nullable: false),
                    claimNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    eventCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    associatedDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    procedureCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    procedureModifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    main = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    minimunBenefitCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createddate = table.Column<DateTime>(type: "datetime", nullable: false),
                    finalDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationClaimEvents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    membershipNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    claimNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendorReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    authNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claimStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requestingVendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requestingProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    provider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claimCreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    claimUpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    claimTotal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payTotal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    adminDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    dischargeDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    coPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    minBenefitCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    batchNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    receiveDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    datasource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    calculatedAdminDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    calculatedDischargeDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    medicalAid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationClaims", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationICD10types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    icdcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    assignmentItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationICD10types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Hypoglycaemias",
                columns: table => new
                {
                    HypoglycaemiaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    glucoseReading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hypoglycaemiaSymptomsCheck = table.Column<bool>(type: "bit", nullable: false),
                    hypoglycaemiaCheck = table.Column<bool>(type: "bit", nullable: false),
                    hypoglycaemiaComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hypoglycaemiaSymptomsComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hypoglycaemiaAssistance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emergencyRoomVisits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    insulinUseCheck = table.Column<bool>(type: "bit", nullable: false),
                    insulinUseComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sulfonylureaUseCheck = table.Column<bool>(type: "bit", nullable: false),
                    sulfonylureaUseComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ckdStageCheck = table.Column<bool>(type: "bit", nullable: false),
                    ckdStageComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    patientAgeCheck = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hypoglycaemias", x => x.HypoglycaemiaID);
                });

            migrationBuilder.CreateTable(
                name: "HypoglymiaRiskHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HypoRisk = table.Column<bool>(type: "bit", nullable: false),
                    Insulin = table.Column<bool>(type: "bit", nullable: false),
                    Sulphonylureas = table.Column<bool>(type: "bit", nullable: false),
                    Glucose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Renal = table.Column<bool>(type: "bit", nullable: false),
                    hypo = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HypoglymiaRiskHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HypoRiskRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ltAge = table.Column<int>(type: "int", nullable: false),
                    gtAge = table.Column<int>(type: "int", nullable: false),
                    Insulin = table.Column<bool>(type: "bit", nullable: false),
                    Sulphonylureas = table.Column<bool>(type: "bit", nullable: false),
                    Glucose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Renal = table.Column<bool>(type: "bit", nullable: false),
                    hypo = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HypoRiskRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "IC10Masters",
                columns: table => new
                {
                    ICD10Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ICD10Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidPrimary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidClinical = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidAsterisk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidDagger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidSequelae = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgeRange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ChapterNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChapterDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC10Masters", x => x.ICD10Code);
                });

            migrationBuilder.CreateTable(
                name: "Icd10Codes",
                columns: table => new
                {
                    icd10CodeID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    codeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    subcode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icd10Codes", x => x.icd10CodeID);
                });

            migrationBuilder.CreateTable(
                name: "ICD10OnDischargeSetups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICD10_on_discharge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICD10OnDischargeSetups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "InsulinDependanceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScriptID = table.Column<int>(type: "int", nullable: true),
                    ScriptItemID = table.Column<int>(type: "int", nullable: true),
                    ScriptItemStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsulinDependanceHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsulinDependances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsulinDependances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "laboratories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    telephoneNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laboratories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "LifeExpectancyRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    smoker = table.Column<bool>(type: "bit", nullable: false),
                    hypertension = table.Column<bool>(type: "bit", nullable: false),
                    hychol = table.Column<bool>(type: "bit", nullable: false),
                    gtHba1C = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ltHba1C = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gtAge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ltAge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeExpectancyRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    LogID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "Logins",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoggedIn = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logins", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "managementStatus_DeactivatedReasons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_managementStatus_DeactivatedReasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidClaimCode",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medicalAidCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    entryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claimCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidClaimCode", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidPlanPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalAidID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    planID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    programID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidPlanPrograms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidProgramIcd10codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalAidID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    icd10Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidProgramIcd10codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidPrograms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    medicalAidId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    referralFrom = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidPrograms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MedicationHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nappiCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    directions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    treatmentNaive = table.Column<bool>(type: "bit", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifieddate = table.Column<DateTime>(type: "datetime", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    QuestionnaireID = table.Column<int>(type: "int", nullable: true),
                    hasMedication = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medicines",
                columns: table => new
                {
                    MedicineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Adherence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeofRegime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientsOnInsulin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sick_day_management = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseOfStatins = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nameOfMedication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeAndDosageOfMedication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    medsInterrupted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changeOfRegimeHIV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MentalChangeOfRegime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SideEffects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    drugOrsubstanceAbuseoperty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicines", x => x.MedicineID);
                });

            migrationBuilder.CreateTable(
                name: "MemberImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    medicalAidCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    membershipNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    option = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    iDNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    addressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    addressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    postalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    telephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cellphone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    optionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    employerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    memberStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    eligibilityStart = table.Column<DateTime>(type: "datetime", nullable: true),
                    eligibilityEnd = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ProgramPopUp = table.Column<bool>(type: "bit", nullable: false),
                    ProfilePopUp = table.Column<bool>(type: "bit", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    memberID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    medicalAidID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    membershipNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.memberID);
                });

            migrationBuilder.CreateTable(
                name: "MembershipNoHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changeDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipNoHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MentalHealthDiagnoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ICD10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConditionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentalHealthDiagnoses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MH_BipolarResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    Depression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepressionSC = table.Column<int>(type: "int", nullable: false),
                    Insomnia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsomniaSC = table.Column<int>(type: "int", nullable: false),
                    Appetite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppetiteSC = table.Column<int>(type: "int", nullable: false),
                    SocialEngagement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SocialEngagementSC = table.Column<int>(type: "int", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivitySC = table.Column<int>(type: "int", nullable: false),
                    Motivation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotivationSC = table.Column<int>(type: "int", nullable: false),
                    Concentration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcentrationSC = table.Column<int>(type: "int", nullable: false),
                    Anxiety = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnxietySC = table.Column<int>(type: "int", nullable: false),
                    Pleasure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PleasureSC = table.Column<int>(type: "int", nullable: false),
                    Emotion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmotionSC = table.Column<int>(type: "int", nullable: false),
                    Worthlessness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorthlessnessSC = table.Column<int>(type: "int", nullable: false),
                    Helplessness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HelplessnessSC = table.Column<int>(type: "int", nullable: false),
                    Suicidal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuicidalSC = table.Column<int>(type: "int", nullable: false),
                    SuicidalCMT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guilt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuiltSC = table.Column<int>(type: "int", nullable: false),
                    Psychotic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PsychoticSC = table.Column<int>(type: "int", nullable: false),
                    Irritability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IrritabilitySC = table.Column<int>(type: "int", nullable: false),
                    Lability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabilitySC = table.Column<int>(type: "int", nullable: false),
                    IncMotorDrive = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncMotorDriveSC = table.Column<int>(type: "int", nullable: false),
                    IncSpeech = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncSpeechSC = table.Column<int>(type: "int", nullable: false),
                    Agitation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgitationSC = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_BipolarResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MH_DepressionResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    Depression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepressionSC = table.Column<int>(type: "int", nullable: false),
                    Guilt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuiltSC = table.Column<int>(type: "int", nullable: false),
                    Suicidal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuicidalSC = table.Column<int>(type: "int", nullable: false),
                    InsomniaEarlyNight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsomniaEarlyNightSC = table.Column<int>(type: "int", nullable: false),
                    InsomniaMiddleNight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsomniaMiddleNightSC = table.Column<int>(type: "int", nullable: false),
                    InsomniaEarlyMorning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsomniaEarlyMorningSC = table.Column<int>(type: "int", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivitySC = table.Column<int>(type: "int", nullable: false),
                    Psychomotor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PsychomotorSC = table.Column<int>(type: "int", nullable: false),
                    Agitation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgitationSC = table.Column<int>(type: "int", nullable: false),
                    AnxietyPsychic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnxietyPsychicSC = table.Column<int>(type: "int", nullable: false),
                    AnxietySomatic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnxietySomaticSC = table.Column<int>(type: "int", nullable: false),
                    SomaticSymGastro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SomaticSymGastroSC = table.Column<int>(type: "int", nullable: false),
                    SomaticSymGeneral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SomaticSymGeneralSC = table.Column<int>(type: "int", nullable: false),
                    Sexology = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SexologySC = table.Column<int>(type: "int", nullable: false),
                    Hypochondriasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HypochondriasisSC = table.Column<int>(type: "int", nullable: false),
                    WeightLoss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeightLossSC = table.Column<int>(type: "int", nullable: false),
                    Insight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsightSC = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_DepressionResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MH_DSM5Responses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    SubstanceAbuse = table.Column<bool>(type: "bit", nullable: false),
                    Depression = table.Column<bool>(type: "bit", nullable: false),
                    DepressionSC = table.Column<int>(type: "int", nullable: false),
                    Interest = table.Column<bool>(type: "bit", nullable: false),
                    InterestSC = table.Column<int>(type: "int", nullable: false),
                    WeightLoss = table.Column<bool>(type: "bit", nullable: false),
                    WeightLossSC = table.Column<int>(type: "int", nullable: false),
                    Psychomotor = table.Column<bool>(type: "bit", nullable: false),
                    PsychomotorSC = table.Column<int>(type: "int", nullable: false),
                    Tiredness = table.Column<bool>(type: "bit", nullable: false),
                    TirednessSC = table.Column<int>(type: "int", nullable: false),
                    SelfWorth = table.Column<bool>(type: "bit", nullable: false),
                    SelfWorthSC = table.Column<int>(type: "int", nullable: false),
                    Concentration = table.Column<bool>(type: "bit", nullable: false),
                    ConcentrationSC = table.Column<int>(type: "int", nullable: false),
                    Suicidal = table.Column<bool>(type: "bit", nullable: false),
                    SuicidalSC = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_DSM5Responses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MH_DSM5ResponsesNEW",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    SubstanceAbuse = table.Column<bool>(type: "bit", nullable: false),
                    DepressionOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepressionOneSC = table.Column<int>(type: "int", nullable: false),
                    DepressionTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepressionTwoSC = table.Column<int>(type: "int", nullable: false),
                    AngerOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AngerOneSC = table.Column<int>(type: "int", nullable: false),
                    ManiaOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManiaOneSC = table.Column<int>(type: "int", nullable: false),
                    ManiaTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManiaTwoSC = table.Column<int>(type: "int", nullable: false),
                    AnxietyOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnxietyOneSC = table.Column<int>(type: "int", nullable: false),
                    AnxietyTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnxietyTwoSC = table.Column<int>(type: "int", nullable: false),
                    AnxietyThree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnxietyThreeSC = table.Column<int>(type: "int", nullable: false),
                    SomaticOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SomaticOneSC = table.Column<int>(type: "int", nullable: false),
                    SomaticTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SomaticTwoSC = table.Column<int>(type: "int", nullable: false),
                    SuicidalOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuicidalOneSC = table.Column<int>(type: "int", nullable: false),
                    PsychosisOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PsychosisOneSC = table.Column<int>(type: "int", nullable: false),
                    PsychosisTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PsychosisTwoSC = table.Column<int>(type: "int", nullable: false),
                    SleepOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SleepOneSC = table.Column<int>(type: "int", nullable: false),
                    MemoryOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MemoryOneSC = table.Column<int>(type: "int", nullable: false),
                    BehaviourOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BehaviourOneSC = table.Column<int>(type: "int", nullable: false),
                    BehaviourTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BehaviourTwoSC = table.Column<int>(type: "int", nullable: false),
                    DissociationOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DissociationOneSC = table.Column<int>(type: "int", nullable: false),
                    PersonalityOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalityOneSC = table.Column<int>(type: "int", nullable: false),
                    PersonalityTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalityTwoSC = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true),
                    DSM5ScoreID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_DSM5ResponsesNEW", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MH_DSM5Scores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_DSM5Scores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MH_SchizophreniaResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    Depression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepressionSC = table.Column<int>(type: "int", nullable: false),
                    Hopelessness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HopelessnessSC = table.Column<int>(type: "int", nullable: false),
                    SelfDepreciation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelfDepreciationSC = table.Column<int>(type: "int", nullable: false),
                    GuiltyIdeas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuiltyIdeasSC = table.Column<int>(type: "int", nullable: false),
                    PathologicalGuilt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathologicalGuiltSC = table.Column<int>(type: "int", nullable: false),
                    MorningDepression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MorningDepressionSC = table.Column<int>(type: "int", nullable: false),
                    EarlyWakening = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EarlyWakeningSC = table.Column<int>(type: "int", nullable: false),
                    Suicidal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuicidalSC = table.Column<int>(type: "int", nullable: false),
                    SuicidalCMT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObservedDepression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObservedDepressionSC = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_SchizophreniaResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MHRiskRatingRules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Depression = table.Column<bool>(type: "bit", nullable: false),
                    Schizophrenia = table.Column<bool>(type: "bit", nullable: false),
                    Bipolar = table.Column<bool>(type: "bit", nullable: false),
                    ltValue = table.Column<int>(type: "int", nullable: false),
                    gtValue = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    suicidal = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MHRiskRatingRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "NewBorns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    hasConcern = table.Column<bool>(type: "bit", nullable: false),
                    Concern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isBreastfeeding = table.Column<bool>(type: "bit", nullable: false),
                    Breastfeeding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isOnMedication = table.Column<bool>(type: "bit", nullable: false),
                    Medication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hivTest = table.Column<bool>(type: "bit", nullable: false),
                    HIVTestComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hivResults = table.Column<bool>(type: "bit", nullable: false),
                    HIVResultsComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    GeneralComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewBorns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NextOFKins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NextOFKins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonCLDFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    flagCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonCLDFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    noteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    noteType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    importNotesSubjet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    expiryDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    special = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    SentToCL = table.Column<int>(type: "int", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrescriberPr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrescriberName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.noteID);
                });

            migrationBuilder.CreateTable(
                name: "NoteTypes",
                columns: table => new
                {
                    noteType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    noteDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteTypes", x => x.noteType);
                });

            migrationBuilder.CreateTable(
                name: "nutritionAndLifestyles",
                columns: table => new
                {
                    nutritionAndLifestyleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dietHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    healthyEatingHabits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    exerciseAndActivity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    weightManagementAddressed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SleepPattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutritionAndLifestyles", x => x.nutritionAndLifestyleID);
                });

            migrationBuilder.CreateTable(
                name: "Origins",
                columns: table => new
                {
                    OriginID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    originName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origins", x => x.OriginID);
                });

            migrationBuilder.CreateTable(
                name: "otherMedicalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    respiratoryTract = table.Column<bool>(type: "bit", nullable: false),
                    respiratoryTractComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    respiratoryTractEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    pneumonia = table.Column<bool>(type: "bit", nullable: false),
                    pneumoniaComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pneumoniaEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    meningitis = table.Column<bool>(type: "bit", nullable: false),
                    meningitisComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    meningitisEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    lymphadenopathy = table.Column<bool>(type: "bit", nullable: false),
                    lymphadenopathyComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lymphadenopathyEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    diarrhoea = table.Column<bool>(type: "bit", nullable: false),
                    diarrhoeaComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diarrhoeaEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    bladderInfection = table.Column<bool>(type: "bit", nullable: false),
                    bladderInfectionComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bladderInfectionEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    weightLoss = table.Column<bool>(type: "bit", nullable: false),
                    weightLossComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    weightLossEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    cancer = table.Column<bool>(type: "bit", nullable: false),
                    cancerComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cancerEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    immunization = table.Column<bool>(type: "bit", nullable: false),
                    immunizationComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    immunizationEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    cervicalCancer = table.Column<bool>(type: "bit", nullable: false),
                    cervicalCancerComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cervicalCancerEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otherMedicalHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "paediatrics",
                columns: table => new
                {
                    paediatricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    currentWeight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RxDoseIn_lineWithWeight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    syrupVsTablets_ableToSwallow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    statusAwareness = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paediatrics", x => x.paediatricID);
                });

            migrationBuilder.CreateTable(
                name: "Pathology",
                columns: table => new
                {
                    pathologyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    pathologyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CD4Count = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CD4CounteffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CD4Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CD4PercentageeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    viralLoad = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    viralLoadeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    haemoglobin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    haemoglobineffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    bilirubin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    bilirubineffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    totalCholestrol = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    totalCholestroleffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    hdl = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    hdleffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ldl = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ldleffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    triglycerides = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    triglycerideseffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    glucose = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    glucoseeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    hba1c = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    hba1ceffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    alt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    alteffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ast = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    asteffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    urea = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ureaeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    creatinine = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    creatinineeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    labName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    labReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    labTelNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    eGfr = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    eGfreffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    mauCreatRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    mauCreatRatioeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    systolicBP = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    diastolicBP = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BPeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    FEV1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEV1effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Eosinophylia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EosinophyliaeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    hivEliza = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hivElizaeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    normGtt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    normGtteffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    abnGtt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    abnGtteffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ucreatinine = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ucreatinineeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    salbumin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    salbumineffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ualbuminuria = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ualbuminuriaeffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    sentToCL = table.Column<int>(type: "int", nullable: false),
                    stageChecked = table.Column<bool>(type: "bit", nullable: false),
                    HypoChecked = table.Column<bool>(type: "bit", nullable: false),
                    egfrChecked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pathology", x => x.pathologyID);
                });

            migrationBuilder.CreateTable(
                name: "PathologyAssignments",
                columns: table => new
                {
                    PathologyAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathologyField = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignmentItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathologyDueDate = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathologyAssignments", x => x.PathologyAssignmentId);
                });

            migrationBuilder.CreateTable(
                name: "pathologyAutomatedTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedSchemes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedPrograms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedManagmentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathologyAutomatedTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PathologyIndex",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    sentToCL = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathologyIndex", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PathologyItems",
                columns: table => new
                {
                    itemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pathologyId = table.Column<int>(type: "int", nullable: false),
                    test = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    result = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    currentValue = table.Column<bool>(type: "bit", nullable: false),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sentToCl = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathologyItems", x => x.itemId);
                });

            migrationBuilder.CreateTable(
                name: "PathologyTestTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathologyTestTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PathologyTypes",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    pathologyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathologyTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientAddressHistories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    addressId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAddressHistories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientClinicalRulesHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ruleBroken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    pathologyID = table.Column<int>(type: "int", nullable: false),
                    assignmentItem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pathFieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pathFieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientClinicalRulesHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientContactHistories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contactId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientContactHistories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientDiagnoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProgramCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ICD10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConditionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    FollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Medication = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDiagnoses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientDisclaimers",
                columns: table => new
                {
                    PatientDisclaimerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignedAcknowledgement = table.Column<bool>(type: "bit", nullable: false),
                    TelephonicAcknowledgement = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDisclaimers", x => x.PatientDisclaimerId);
                });

            migrationBuilder.CreateTable(
                name: "PatientDispensingProviderHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dispensingId = table.Column<int>(type: "int", nullable: false),
                    dispensingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDispensingProviderHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientDoctorHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    doctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    doctorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDoctorHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientPlanHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    planId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    planName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPlanHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientProgramHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    icd10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    conditionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProgramHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientProgramSubHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    icd10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    conditionCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProgramSubHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientQuestionnaireResponses",
                columns: table => new
                {
                    PatientQuestionnaireResponseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    Answer = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientQuestionnaireResponses", x => x.PatientQuestionnaireResponseID);
                });

            migrationBuilder.CreateTable(
                name: "PatientRiskRatingHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sentToCl = table.Column<int>(type: "int", nullable: false),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    patientStagingHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientRiskRatingHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientStagingHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    stageCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cd4Count = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    cd4Percentage = table.Column<int>(type: "int", nullable: true),
                    viralLoad = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientStagingHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PatientStatus",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientStatus", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "PatientStatusHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    statusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientStatusHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PayPointHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    planId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    planName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayPointHistory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PayPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    medicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Podiatries",
                columns: table => new
                {
                    PodiatryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    amputationComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amputationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amputationCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryLopsCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryLopsComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryDeformityCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryDeformityComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryPerArterialDiseaseCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryPerArterialDiseaseAffectedLeg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryPerArterialDiseaseComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryWoundCheck = table.Column<bool>(type: "bit", nullable: false),
                    podiatryWoundAffectedLeg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podiatryWoundComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WoundInfection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Podiatries", x => x.PodiatryID);
                });

            migrationBuilder.CreateTable(
                name: "PopUpTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedSchemes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    selectedPrograms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PopUpTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Practices",
                columns: table => new
                {
                    practiceNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    practiceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    councilNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    service = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    decipline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Practices", x => x.practiceNo);
                });

            migrationBuilder.CreateTable(
                name: "preferredMethodOfContacts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pmocCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    pmocDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preferredMethodOfContacts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Priorities",
                columns: table => new
                {
                    prioritytype = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    priorityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priorities", x => x.prioritytype);
                });

            migrationBuilder.CreateTable(
                name: "ProductionWorkItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionWorkItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    nappiCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    productName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    packSize = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Insulin = table.Column<bool>(type: "bit", nullable: false),
                    Sulponylureas = table.Column<bool>(type: "bit", nullable: false),
                    atcCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    therapeuticClass6Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    therapeuticClass6 = table.Column<int>(type: "int", nullable: false),
                    mmapIndicator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mrpIndicator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    packUOM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    genericIndicator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productSchedule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    strength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    therapeuticClass12 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dosageForm = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.nappiCode);
                });

            migrationBuilder.CreateTable(
                name: "ProfileLocks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileLocks", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ProgramICD10Codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icd10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramICD10Codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queries",
                columns: table => new
                {
                    queryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    queryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    enquiryBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    querySubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    querySolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    resolvedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resolutionDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    queryStatus = table.Column<bool>(type: "bit", nullable: false),
                    followUpDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queries", x => x.queryID);
                });

            migrationBuilder.CreateTable(
                name: "QueryTypes",
                columns: table => new
                {
                    code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    queryDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryTypes", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireOthers",
                columns: table => new
                {
                    QuestionnaireOtherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    occupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    occupationCheck = table.Column<bool>(type: "bit", nullable: false),
                    shiftWorker = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shiftWorkCheck = table.Column<bool>(type: "bit", nullable: false),
                    lypohypertrophy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lypohypertrophyCheck = table.Column<bool>(type: "bit", nullable: false),
                    lypohypertrophyDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    recDrugs = table.Column<bool>(type: "bit", nullable: false),
                    recDrugsLastUsed = table.Column<DateTime>(type: "datetime", nullable: true),
                    depression = table.Column<bool>(type: "bit", nullable: false),
                    depressionComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TBScreen = table.Column<bool>(type: "bit", nullable: false),
                    TBScreenDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    TBScreenResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tbDiagnosed = table.Column<bool>(type: "bit", nullable: false),
                    tbTreatmentStartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    tbTreatmentEndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Pregnant = table.Column<bool>(type: "bit", nullable: false),
                    EDD = table.Column<DateTime>(type: "datetime", nullable: true),
                    TreatingDoctor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isDoctorAware = table.Column<bool>(type: "bit", nullable: false),
                    frailCareCheck = table.Column<bool>(type: "bit", nullable: false),
                    frailCare = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    frailCareComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    hasKidneyInfection = table.Column<bool>(type: "bit", nullable: false),
                    bloodTestFrequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bloodTestEffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireOthers", x => x.QuestionnaireOtherID);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireTemplates",
                columns: table => new
                {
                    QuestionnaireTemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionNo = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Answer = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireTemplates", x => x.QuestionnaireTemplateID);
                });

            migrationBuilder.CreateTable(
                name: "referralFroms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    referral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referralFroms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "referrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    referral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referrals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskRatingMonitors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskRatingMonitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskReportingData",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    month = table.Column<int>(type: "int", nullable: false),
                    area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    other = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valueInt = table.Column<int>(type: "int", nullable: true),
                    valueDouble = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskReportingData", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RoleAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    admin = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    adminRights = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchemeAccess",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Authorizations = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemeAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScriptItems",
                columns: table => new
                {
                    itemNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    scriptID = table.Column<int>(type: "int", nullable: false),
                    nappiCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    directions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fromDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    toDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    itemRepeats = table.Column<int>(type: "int", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    itemStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prophylaxis = table.Column<bool>(type: "bit", nullable: false),
                    itemType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    benefitType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    coPaymentOverride = table.Column<bool>(type: "bit", nullable: false),
                    icd10code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CLItemLineNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptItems", x => x.itemNo);
                });

            migrationBuilder.CreateTable(
                name: "ScriptItemTypes",
                columns: table => new
                {
                    itemType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    itemTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptItemTypes", x => x.itemType);
                });

            migrationBuilder.CreateTable(
                name: "ScriptReferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reference = table.Column<int>(type: "int", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    attachment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptReferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Scripts",
                columns: table => new
                {
                    scriptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    scriptType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    repeats = table.Column<int>(type: "int", nullable: false),
                    doctorID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    firstLine = table.Column<bool>(type: "bit", nullable: false),
                    secondLine = table.Column<bool>(type: "bit", nullable: false),
                    salvageTherapy = table.Column<bool>(type: "bit", nullable: false),
                    prophylaxis = table.Column<bool>(type: "bit", nullable: false),
                    resistanceTest = table.Column<bool>(type: "bit", nullable: false),
                    practiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scripts", x => x.scriptID);
                });

            migrationBuilder.CreateTable(
                name: "ScriptTypes",
                columns: table => new
                {
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    typeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptTypes", x => x.type);
                });

            migrationBuilder.CreateTable(
                name: "Sex",
                columns: table => new
                {
                    sex = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sexName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sex", x => x.sex);
                });

            migrationBuilder.CreateTable(
                name: "SmsMessages",
                columns: table => new
                {
                    messageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cell_no = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    templateID = table.Column<int>(type: "int", nullable: false),
                    importSMSSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AttuneID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    responseString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttuneMessageStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttuneDeliveryDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMessages", x => x.messageID);
                });

            migrationBuilder.CreateTable(
                name: "SmsMessageTemplates",
                columns: table => new
                {
                    templateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    smsLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    program = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMessageTemplates", x => x.templateID);
                });

            migrationBuilder.CreateTable(
                name: "StatusTypes",
                columns: table => new
                {
                    TypeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusTypes", x => x.TypeCode);
                });

            migrationBuilder.CreateTable(
                name: "TariffClaimHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tariff = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TariffDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfClaim = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifieddate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    programID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffClaimHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TariffCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TariffBillingCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TariffDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pathology = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifieddate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskRequirementItemLinking",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    itemId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    taskType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requirementId = table.Column<int>(type: "int", nullable: false),
                    templateID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    taskSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskRequirementItemLinking", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TaskTypeRequirements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    requirementText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    templateID = table.Column<int>(type: "int", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTypeRequirements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserMemberHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentID = table.Column<int>(type: "int", nullable: true),
                    TaskID = table.Column<int>(type: "int", nullable: true),
                    UserFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMemberHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProgramAccess",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    medicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgramAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    roleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleRights",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    accessId = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleRights", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleWorkflowRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignmentTypeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleWorkflowRights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSchemeAccess",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    medicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSchemeAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Visions",
                columns: table => new
                {
                    VisionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    questionnaireID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    vision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visionCheck = table.Column<bool>(type: "bit", nullable: false),
                    eyeTreatmentCheck = table.Column<bool>(type: "bit", nullable: false),
                    eyeTreatment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visions", x => x.VisionID);
                });

            migrationBuilder.CreateTable(
                name: "visits",
                columns: table => new
                {
                    visitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowUpDoctor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUpClinic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUpHaloCare = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assignementSent = table.Column<bool>(type: "bit", nullable: false),
                    generalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowUpPsychologist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUpPsychiatrist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visits", x => x.visitID);
                });

            migrationBuilder.CreateTable(
                name: "wfQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchemeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    MembershipNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wfSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalAidID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    PathologyField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Less = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Greater = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wfUserLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfUserLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wfUserQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShuffleDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ShuffleCount = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfUserQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalScheme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MembershipNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InProgress = table.Column<bool>(type: "bit", nullable: false),
                    InProgressDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    InProgressBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Closed = table.Column<bool>(type: "bit", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalAidID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RiskRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalScheme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MembershipNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagementStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Open = table.Column<bool>(type: "bit", nullable: false),
                    InProgress = table.Column<bool>(type: "bit", nullable: false),
                    Closed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentItemTypes",
                columns: table => new
                {
                    assignmentItemType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    itemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sourceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    followUp = table.Column<bool>(type: "bit", nullable: false),
                    capture = table.Column<bool>(type: "bit", nullable: false),
                    attach = table.Column<bool>(type: "bit", nullable: false),
                    requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diabetes = table.Column<bool>(type: "bit", nullable: true),
                    hiv = table.Column<bool>(type: "bit", nullable: true),
                    oncology = table.Column<bool>(type: "bit", nullable: true),
                    mental = table.Column<bool>(type: "bit", nullable: true),
                    hypertension = table.Column<bool>(type: "bit", nullable: true),
                    TB = table.Column<bool>(type: "bit", nullable: true),
                    ClinicalRulesid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentItemTypes", x => x.assignmentItemType);
                    table.ForeignKey(
                        name: "FK_AssignmentItemTypes_ClinicalRules_ClinicalRulesid",
                        column: x => x.ClinicalRulesid,
                        principalTable: "ClinicalRules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pathologyFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PathologyField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    ClinicalRulesid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathologyFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pathologyFields_ClinicalRules_ClinicalRulesid",
                        column: x => x.ClinicalRulesid,
                        principalTable: "ClinicalRules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    FileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_EmailAttachments_Emails_EmailID",
                        column: x => x.EmailID,
                        principalTable: "Emails",
                        principalColumn: "emailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MH_DSM5ScoreHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldScore = table.Column<int>(type: "int", nullable: false),
                    NewScore = table.Column<int>(type: "int", nullable: false),
                    OldReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    RiskID = table.Column<int>(type: "int", nullable: true),
                    MH_DSM5ScoreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MH_DSM5ScoreHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MH_DSM5ScoreHistories_MH_DSM5Scores_MH_DSM5ScoreId",
                        column: x => x.MH_DSM5ScoreId,
                        principalTable: "MH_DSM5Scores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManagementStatus",
                columns: table => new
                {
                    statusCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    statusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    statusType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    programCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    enrolmentStage = table.Column<int>(type: "int", nullable: false),
                    PathologyAutomatedTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementStatus", x => x.statusCode);
                    table.ForeignKey(
                        name: "FK_ManagementStatus_pathologyAutomatedTemplates_PathologyAutomatedTemplateId",
                        column: x => x.PathologyAutomatedTemplateId,
                        principalTable: "pathologyAutomatedTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    medicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    planCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifieddate = table.Column<DateTime>(type: "datetime", nullable: true),
                    claimCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    servicePlanCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathologyAutomatedTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PopUpTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalAidPlans_pathologyAutomatedTemplates_PathologyAutomatedTemplateId",
                        column: x => x.PathologyAutomatedTemplateId,
                        principalTable: "pathologyAutomatedTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalAidPlans_PopUpTemplates_PopUpTemplateId",
                        column: x => x.PopUpTemplateId,
                        principalTable: "PopUpTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAids",
                columns: table => new
                {
                    MedicalAidID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    medicalAidCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    logoLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    clCarrier = table.Column<bool>(type: "bit", nullable: false),
                    riskRules = table.Column<bool>(type: "bit", nullable: false),
                    reporting = table.Column<bool>(type: "bit", nullable: false),
                    titleSync = table.Column<bool>(type: "bit", nullable: false),
                    disclaimer = table.Column<bool>(type: "bit", nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Production = table.Column<bool>(type: "bit", nullable: false),
                    Extracts = table.Column<bool>(type: "bit", nullable: false),
                    Imports = table.Column<bool>(type: "bit", nullable: false),
                    Exports = table.Column<bool>(type: "bit", nullable: false),
                    AdvancedSearch = table.Column<bool>(type: "bit", nullable: false),
                    Claims = table.Column<bool>(type: "bit", nullable: false),
                    smsoptout = table.Column<bool>(type: "bit", nullable: false),
                    optOutEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sync = table.Column<bool>(type: "bit", nullable: false),
                    PathologyAutomatedTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PopUpTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    memberID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAids", x => x.MedicalAidID);
                    table.ForeignKey(
                        name: "FK_MedicalAids_Members_memberID",
                        column: x => x.memberID,
                        principalTable: "Members",
                        principalColumn: "memberID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalAids_pathologyAutomatedTemplates_PathologyAutomatedTemplateId",
                        column: x => x.PathologyAutomatedTemplateId,
                        principalTable: "pathologyAutomatedTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalAids_PopUpTemplates_PopUpTemplateId",
                        column: x => x.PopUpTemplateId,
                        principalTable: "PopUpTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskRatingTypes",
                columns: table => new
                {
                    RiskType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RiskName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskPriority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    PatientRiskRatingHistoryid = table.Column<int>(type: "int", nullable: true),
                    RiskRatingMonitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskRatingTypes", x => x.RiskType);
                    table.ForeignKey(
                        name: "FK_RiskRatingTypes_PatientRiskRatingHistory_PatientRiskRatingHistoryid",
                        column: x => x.PatientRiskRatingHistoryid,
                        principalTable: "PatientRiskRatingHistory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskRatingTypes_RiskRatingMonitors_RiskRatingMonitorId",
                        column: x => x.RiskRatingMonitorId,
                        principalTable: "RiskRatingMonitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    languageName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    languageCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    iSeriesLanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    EmailTemplatestemplateID = table.Column<int>(type: "int", nullable: true),
                    SmsMessageTemplatestemplateID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.languageName);
                    table.ForeignKey(
                        name: "FK_Language_EmailTemplates_EmailTemplatestemplateID",
                        column: x => x.EmailTemplatestemplateID,
                        principalTable: "EmailTemplates",
                        principalColumn: "templateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Language_SmsMessageTemplates_SmsMessageTemplatestemplateID",
                        column: x => x.SmsMessageTemplatestemplateID,
                        principalTable: "SmsMessageTemplates",
                        principalColumn: "templateID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Program",
                columns: table => new
                {
                    programID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    icd10code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    multipleICD10 = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClinicalRulesid = table.Column<int>(type: "int", nullable: true),
                    EmailLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmailTemplatestemplateID = table.Column<int>(type: "int", nullable: true),
                    PathologyAutomatedTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PopUpTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RiskRatingMonitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SmsMessageTemplatestemplateID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Program", x => x.programID);
                    table.ForeignKey(
                        name: "FK_Program_AttachmentTemplates_AttachmentTemplateId",
                        column: x => x.AttachmentTemplateId,
                        principalTable: "AttachmentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_ClinicalRules_ClinicalRulesid",
                        column: x => x.ClinicalRulesid,
                        principalTable: "ClinicalRules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_EmailLayouts_EmailLayoutId",
                        column: x => x.EmailLayoutId,
                        principalTable: "EmailLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_EmailTemplates_EmailTemplatestemplateID",
                        column: x => x.EmailTemplatestemplateID,
                        principalTable: "EmailTemplates",
                        principalColumn: "templateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_pathologyAutomatedTemplates_PathologyAutomatedTemplateId",
                        column: x => x.PathologyAutomatedTemplateId,
                        principalTable: "pathologyAutomatedTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_PopUpTemplates_PopUpTemplateId",
                        column: x => x.PopUpTemplateId,
                        principalTable: "PopUpTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_RiskRatingMonitors_RiskRatingMonitorId",
                        column: x => x.RiskRatingMonitorId,
                        principalTable: "RiskRatingMonitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Program_SmsMessageTemplates_SmsMessageTemplatestemplateID",
                        column: x => x.SmsMessageTemplatestemplateID,
                        principalTable: "SmsMessageTemplates",
                        principalColumn: "templateID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    salt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    accountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SearchManagement = table.Column<bool>(type: "bit", nullable: false),
                    UserMemberHistoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userID);
                    table.ForeignKey(
                        name: "FK_Users_UserMemberHistories_UserMemberHistoryId",
                        column: x => x.UserMemberHistoryId,
                        principalTable: "UserMemberHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientManagementStatusHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dependantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    managementStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    sentToCl = table.Column<bool>(type: "bit", nullable: false),
                    ManagementStatusstatusCode = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientManagementStatusHistory", x => x.id);
                    table.ForeignKey(
                        name: "FK_PatientManagementStatusHistory_ManagementStatus_ManagementStatusstatusCode",
                        column: x => x.ManagementStatusstatusCode,
                        principalTable: "ManagementStatus",
                        principalColumn: "statusCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidSettings",
                columns: table => new
                {
                    settingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medicalAidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hivPreAppovalRequired = table.Column<bool>(type: "bit", nullable: false),
                    daysForPathology = table.Column<int>(type: "int", nullable: false),
                    authRequired = table.Column<bool>(type: "bit", nullable: false),
                    daysForCommunicationGen = table.Column<int>(type: "int", nullable: false),
                    daysForCommunicationHiv = table.Column<int>(type: "int", nullable: false),
                    overdueScriptSms = table.Column<int>(type: "int", nullable: false),
                    overduePathologySms = table.Column<int>(type: "int", nullable: false),
                    confidential = table.Column<bool>(type: "bit", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    schemeSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    modifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidSettings", x => x.settingID);
                    table.ForeignKey(
                        name: "FK_MedicalAidSettings_MedicalAids_medicalAidId",
                        column: x => x.medicalAidId,
                        principalTable: "MedicalAids",
                        principalColumn: "MedicalAidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberInformation",
                columns: table => new
                {
                    MemberInformationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    memberID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    dependantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contactID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    addressID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    programID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    caseManagerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doctorID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AllowText = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberInformation", x => x.MemberInformationID);
                    table.ForeignKey(
                        name: "FK_MemberInformation_Address_addressID",
                        column: x => x.addressID,
                        principalTable: "Address",
                        principalColumn: "addressID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberInformation_Contacts_contactID",
                        column: x => x.contactID,
                        principalTable: "Contacts",
                        principalColumn: "ContactID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberInformation_Dependants_dependantID",
                        column: x => x.dependantID,
                        principalTable: "Dependants",
                        principalColumn: "DependantID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberInformation_Members_memberID",
                        column: x => x.memberID,
                        principalTable: "Members",
                        principalColumn: "memberID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberInformation_Program_programID",
                        column: x => x.programID,
                        principalTable: "Program",
                        principalColumn: "programID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentItemTypes_ClinicalRulesid",
                table: "AssignmentItemTypes",
                column: "ClinicalRulesid");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_EmailID",
                table: "EmailAttachments",
                column: "EmailID");

            migrationBuilder.CreateIndex(
                name: "IX_Language_EmailTemplatestemplateID",
                table: "Language",
                column: "EmailTemplatestemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Language_SmsMessageTemplatestemplateID",
                table: "Language",
                column: "SmsMessageTemplatestemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementStatus_PathologyAutomatedTemplateId",
                table: "ManagementStatus",
                column: "PathologyAutomatedTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidPlans_PathologyAutomatedTemplateId",
                table: "MedicalAidPlans",
                column: "PathologyAutomatedTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidPlans_PopUpTemplateId",
                table: "MedicalAidPlans",
                column: "PopUpTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAids_memberID",
                table: "MedicalAids",
                column: "memberID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAids_PathologyAutomatedTemplateId",
                table: "MedicalAids",
                column: "PathologyAutomatedTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAids_PopUpTemplateId",
                table: "MedicalAids",
                column: "PopUpTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidSettings_medicalAidId",
                table: "MedicalAidSettings",
                column: "medicalAidId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberInformation_addressID",
                table: "MemberInformation",
                column: "addressID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInformation_contactID",
                table: "MemberInformation",
                column: "contactID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInformation_dependantID",
                table: "MemberInformation",
                column: "dependantID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInformation_memberID",
                table: "MemberInformation",
                column: "memberID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInformation_programID",
                table: "MemberInformation",
                column: "programID");

            migrationBuilder.CreateIndex(
                name: "IX_MH_DSM5ScoreHistories_MH_DSM5ScoreId",
                table: "MH_DSM5ScoreHistories",
                column: "MH_DSM5ScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_pathologyFields_ClinicalRulesid",
                table: "pathologyFields",
                column: "ClinicalRulesid");

            migrationBuilder.CreateIndex(
                name: "IX_PatientManagementStatusHistory_ManagementStatusstatusCode",
                table: "PatientManagementStatusHistory",
                column: "ManagementStatusstatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_Program_AttachmentTemplateId",
                table: "Program",
                column: "AttachmentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Program_ClinicalRulesid",
                table: "Program",
                column: "ClinicalRulesid");

            migrationBuilder.CreateIndex(
                name: "IX_Program_EmailLayoutId",
                table: "Program",
                column: "EmailLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Program_EmailTemplatestemplateID",
                table: "Program",
                column: "EmailTemplatestemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Program_PathologyAutomatedTemplateId",
                table: "Program",
                column: "PathologyAutomatedTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Program_PopUpTemplateId",
                table: "Program",
                column: "PopUpTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Program_RiskRatingMonitorId",
                table: "Program",
                column: "RiskRatingMonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Program_SmsMessageTemplatestemplateID",
                table: "Program",
                column: "SmsMessageTemplatestemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_RiskRatingTypes_PatientRiskRatingHistoryid",
                table: "RiskRatingTypes",
                column: "PatientRiskRatingHistoryid");

            migrationBuilder.CreateIndex(
                name: "IX_RiskRatingTypes_RiskRatingMonitorId",
                table: "RiskRatingTypes",
                column: "RiskRatingMonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserMemberHistoryId",
                table: "Users",
                column: "UserMemberHistoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessFunctions");

            migrationBuilder.DropTable(
                name: "AccessServices");

            migrationBuilder.DropTable(
                name: "AccountAttachmentTemplates");

            migrationBuilder.DropTable(
                name: "AccountEmailLayouts");

            migrationBuilder.DropTable(
                name: "AccountEmailTemplates");

            migrationBuilder.DropTable(
                name: "AccountManagementStatuses");

            migrationBuilder.DropTable(
                name: "AccountPlanRules");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AccountSchemes");

            migrationBuilder.DropTable(
                name: "AccountTextTemplates");

            migrationBuilder.DropTable(
                name: "Allergies");

            migrationBuilder.DropTable(
                name: "AssignmentItemLogs");

            migrationBuilder.DropTable(
                name: "AssignmentItems");

            migrationBuilder.DropTable(
                name: "AssignmentItemTasks");

            migrationBuilder.DropTable(
                name: "AssignmentItemTaskTypes");

            migrationBuilder.DropTable(
                name: "AssignmentItemTypes");

            migrationBuilder.DropTable(
                name: "assignmentLocks");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "AssignmentTypes");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "AttachmentTypes");

            migrationBuilder.DropTable(
                name: "AuthorisationCodes");

            migrationBuilder.DropTable(
                name: "Authorisations");

            migrationBuilder.DropTable(
                name: "AuthorizationLetters");

            migrationBuilder.DropTable(
                name: "AutoNeropathies");

            migrationBuilder.DropTable(
                name: "BenefitType");

            migrationBuilder.DropTable(
                name: "BulkEmails");

            migrationBuilder.DropTable(
                name: "CarePlanPathologies");

            migrationBuilder.DropTable(
                name: "CaseManagerHistory");

            migrationBuilder.DropTable(
                name: "CaseManagers");

            migrationBuilder.DropTable(
                name: "ChronicMedication");

            migrationBuilder.DropTable(
                name: "ClaimsHistory");

            migrationBuilder.DropTable(
                name: "Clinical");

            migrationBuilder.DropTable(
                name: "clinicalAdditions");

            migrationBuilder.DropTable(
                name: "ClinicalHistoryQuestionaire");

            migrationBuilder.DropTable(
                name: "ClinicalMessageInstructions");

            migrationBuilder.DropTable(
                name: "ClinicalRuleTypes");

            migrationBuilder.DropTable(
                name: "CoMorbidClinicalRules");

            migrationBuilder.DropTable(
                name: "ComorbidConditionExclusions");

            migrationBuilder.DropTable(
                name: "CoMorbidConditions");

            migrationBuilder.DropTable(
                name: "CoMorbidTypeRules");

            migrationBuilder.DropTable(
                name: "CoMorbidTypes");

            migrationBuilder.DropTable(
                name: "Demographic");

            migrationBuilder.DropTable(
                name: "DependantType");

            migrationBuilder.DropTable(
                name: "DependentPrograms");

            migrationBuilder.DropTable(
                name: "DiabeticDairy");

            migrationBuilder.DropTable(
                name: "DisclaimerQuestions");

            migrationBuilder.DropTable(
                name: "DisclaimerResponse");

            migrationBuilder.DropTable(
                name: "DispensingProviders");

            migrationBuilder.DropTable(
                name: "DoctorInformation");

            migrationBuilder.DropTable(
                name: "DoctorQuestionnaireResults");

            migrationBuilder.DropTable(
                name: "DoctorReferrals");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "DoctorsInformation");

            migrationBuilder.DropTable(
                name: "DoctorTypes");

            migrationBuilder.DropTable(
                name: "EducationalNotes");

            migrationBuilder.DropTable(
                name: "emailAttachmentHistories");

            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "EmployerGroups");

            migrationBuilder.DropTable(
                name: "EmployerMasters");

            migrationBuilder.DropTable(
                name: "EnquiryComments");

            migrationBuilder.DropTable(
                name: "EnquiryTypes");

            migrationBuilder.DropTable(
                name: "EnrolmentStepsMonitor");

            migrationBuilder.DropTable(
                name: "ForgottenPasswords");

            migrationBuilder.DropTable(
                name: "Functions");

            migrationBuilder.DropTable(
                name: "Hba1cRangeHistory");

            migrationBuilder.DropTable(
                name: "HC_Member_Claims");

            migrationBuilder.DropTable(
                name: "HivcomorbidRules");

            migrationBuilder.DropTable(
                name: "HIVRiskRatingRules");

            migrationBuilder.DropTable(
                name: "HIVStages");

            migrationBuilder.DropTable(
                name: "HIVStagingRiskRules");

            migrationBuilder.DropTable(
                name: "HospitalizationAuthEvents");

            migrationBuilder.DropTable(
                name: "HospitalizationAuths");

            migrationBuilder.DropTable(
                name: "HospitalizationClaimEvents");

            migrationBuilder.DropTable(
                name: "HospitalizationClaims");

            migrationBuilder.DropTable(
                name: "HospitalizationICD10types");

            migrationBuilder.DropTable(
                name: "Hypoglycaemias");

            migrationBuilder.DropTable(
                name: "HypoglymiaRiskHistory");

            migrationBuilder.DropTable(
                name: "HypoRiskRules");

            migrationBuilder.DropTable(
                name: "IC10Masters");

            migrationBuilder.DropTable(
                name: "Icd10Codes");

            migrationBuilder.DropTable(
                name: "ICD10OnDischargeSetups");

            migrationBuilder.DropTable(
                name: "InsulinDependanceHistories");

            migrationBuilder.DropTable(
                name: "InsulinDependances");

            migrationBuilder.DropTable(
                name: "laboratories");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "LifeExpectancyRules");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Logins");

            migrationBuilder.DropTable(
                name: "managementStatus_DeactivatedReasons");

            migrationBuilder.DropTable(
                name: "MedicalAidAccounts");

            migrationBuilder.DropTable(
                name: "MedicalAidClaimCode");

            migrationBuilder.DropTable(
                name: "MedicalAidPlanPrograms");

            migrationBuilder.DropTable(
                name: "MedicalAidPlans");

            migrationBuilder.DropTable(
                name: "MedicalAidProgramIcd10codes");

            migrationBuilder.DropTable(
                name: "MedicalAidPrograms");

            migrationBuilder.DropTable(
                name: "MedicalAidSettings");

            migrationBuilder.DropTable(
                name: "MedicationHistory");

            migrationBuilder.DropTable(
                name: "medicines");

            migrationBuilder.DropTable(
                name: "MemberImports");

            migrationBuilder.DropTable(
                name: "MemberInformation");

            migrationBuilder.DropTable(
                name: "MemberRecords");

            migrationBuilder.DropTable(
                name: "MembershipNoHistory");

            migrationBuilder.DropTable(
                name: "MentalHealthDiagnoses");

            migrationBuilder.DropTable(
                name: "MH_BipolarResponses");

            migrationBuilder.DropTable(
                name: "MH_DepressionResponses");

            migrationBuilder.DropTable(
                name: "MH_DSM5Responses");

            migrationBuilder.DropTable(
                name: "MH_DSM5ResponsesNEW");

            migrationBuilder.DropTable(
                name: "MH_DSM5ScoreHistories");

            migrationBuilder.DropTable(
                name: "MH_SchizophreniaResponses");

            migrationBuilder.DropTable(
                name: "MHRiskRatingRules");

            migrationBuilder.DropTable(
                name: "NewBorns");

            migrationBuilder.DropTable(
                name: "NextOFKins");

            migrationBuilder.DropTable(
                name: "NonCLDFlags");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "NoteTypes");

            migrationBuilder.DropTable(
                name: "nutritionAndLifestyles");

            migrationBuilder.DropTable(
                name: "Origins");

            migrationBuilder.DropTable(
                name: "otherMedicalHistories");

            migrationBuilder.DropTable(
                name: "paediatrics");

            migrationBuilder.DropTable(
                name: "Pathology");

            migrationBuilder.DropTable(
                name: "PathologyAssignments");

            migrationBuilder.DropTable(
                name: "pathologyFields");

            migrationBuilder.DropTable(
                name: "PathologyIndex");

            migrationBuilder.DropTable(
                name: "PathologyItems");

            migrationBuilder.DropTable(
                name: "PathologyTestTypes");

            migrationBuilder.DropTable(
                name: "PathologyTypes");

            migrationBuilder.DropTable(
                name: "PatientAddressHistories");

            migrationBuilder.DropTable(
                name: "PatientClinicalRulesHistory");

            migrationBuilder.DropTable(
                name: "PatientContactHistories");

            migrationBuilder.DropTable(
                name: "PatientDiagnoses");

            migrationBuilder.DropTable(
                name: "PatientDisclaimers");

            migrationBuilder.DropTable(
                name: "PatientDispensingProviderHistory");

            migrationBuilder.DropTable(
                name: "PatientDoctorHistory");

            migrationBuilder.DropTable(
                name: "PatientManagementStatusHistory");

            migrationBuilder.DropTable(
                name: "PatientPlanHistory");

            migrationBuilder.DropTable(
                name: "PatientProgramHistory");

            migrationBuilder.DropTable(
                name: "PatientProgramSubHistory");

            migrationBuilder.DropTable(
                name: "PatientQuestionnaireResponses");

            migrationBuilder.DropTable(
                name: "PatientStagingHistory");

            migrationBuilder.DropTable(
                name: "PatientStatus");

            migrationBuilder.DropTable(
                name: "PatientStatusHistory");

            migrationBuilder.DropTable(
                name: "PayPointHistory");

            migrationBuilder.DropTable(
                name: "PayPoints");

            migrationBuilder.DropTable(
                name: "Podiatries");

            migrationBuilder.DropTable(
                name: "Practices");

            migrationBuilder.DropTable(
                name: "preferredMethodOfContacts");

            migrationBuilder.DropTable(
                name: "Priorities");

            migrationBuilder.DropTable(
                name: "ProductionWorkItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProfileLocks");

            migrationBuilder.DropTable(
                name: "ProgramICD10Codes");

            migrationBuilder.DropTable(
                name: "ProgramStatuses");

            migrationBuilder.DropTable(
                name: "Queries");

            migrationBuilder.DropTable(
                name: "QueryTypes");

            migrationBuilder.DropTable(
                name: "QuestionnaireOthers");

            migrationBuilder.DropTable(
                name: "QuestionnaireTemplates");

            migrationBuilder.DropTable(
                name: "referralFroms");

            migrationBuilder.DropTable(
                name: "referrals");

            migrationBuilder.DropTable(
                name: "RiskRatingTypes");

            migrationBuilder.DropTable(
                name: "RiskReportingData");

            migrationBuilder.DropTable(
                name: "RoleAccess");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "SchemeAccess");

            migrationBuilder.DropTable(
                name: "ScriptItems");

            migrationBuilder.DropTable(
                name: "ScriptItemTypes");

            migrationBuilder.DropTable(
                name: "ScriptReferences");

            migrationBuilder.DropTable(
                name: "Scripts");

            migrationBuilder.DropTable(
                name: "ScriptTypes");

            migrationBuilder.DropTable(
                name: "Sex");

            migrationBuilder.DropTable(
                name: "SmsMessages");

            migrationBuilder.DropTable(
                name: "StatusTypes");

            migrationBuilder.DropTable(
                name: "TariffClaimHistories");

            migrationBuilder.DropTable(
                name: "TariffCodes");

            migrationBuilder.DropTable(
                name: "TaskRequirementItemLinking");

            migrationBuilder.DropTable(
                name: "TaskTypeRequirements");

            migrationBuilder.DropTable(
                name: "UserProgramAccess");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "UserRoleRights");

            migrationBuilder.DropTable(
                name: "UserRoleWorkflowRights");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserSchemeAccess");

            migrationBuilder.DropTable(
                name: "Visions");

            migrationBuilder.DropTable(
                name: "visits");

            migrationBuilder.DropTable(
                name: "wfQueues");

            migrationBuilder.DropTable(
                name: "wfSettings");

            migrationBuilder.DropTable(
                name: "wfUserLogs");

            migrationBuilder.DropTable(
                name: "wfUserQueues");

            migrationBuilder.DropTable(
                name: "WorkflowLogs");

            migrationBuilder.DropTable(
                name: "WorkflowSettings");

            migrationBuilder.DropTable(
                name: "WorkflowUsers");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "MedicalAids");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Dependants");

            migrationBuilder.DropTable(
                name: "Program");

            migrationBuilder.DropTable(
                name: "MH_DSM5Scores");

            migrationBuilder.DropTable(
                name: "ManagementStatus");

            migrationBuilder.DropTable(
                name: "PatientRiskRatingHistory");

            migrationBuilder.DropTable(
                name: "UserMemberHistories");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "AttachmentTemplates");

            migrationBuilder.DropTable(
                name: "ClinicalRules");

            migrationBuilder.DropTable(
                name: "EmailLayouts");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "PopUpTemplates");

            migrationBuilder.DropTable(
                name: "RiskRatingMonitors");

            migrationBuilder.DropTable(
                name: "SmsMessageTemplates");

            migrationBuilder.DropTable(
                name: "pathologyAutomatedTemplates");
        }
    }
}
