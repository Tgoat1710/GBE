# Vaccination Campaign API Endpoints

This document outlines all the vaccination campaign API endpoints implemented in the system.

## Authentication & Authorization

All endpoints use role-based authorization:
- **Manager**: Can manage campaigns, view reports, assign nurses
- **Parent**: Can view and respond to consent forms for their children
- **Nurse**: Can view assignments, mark attendance, record vaccinations

## Parent Endpoints

### Get Parent's Consent Forms
```http
GET /api/vaccination/consents/parent/{parentId}
Authorization: RequireParentRole
```
Returns all consent forms for a specific parent.

### Get Consent Details
```http
GET /api/vaccination/consents/{id}
Authorization: RequireParentRole
```
Returns detailed information about a specific consent form.

### Respond to Consent Form
```http
PUT /api/vaccination/consents/{id}/respond
Authorization: RequireParentRole
Content-Type: application/json

{
  "consentStatus": true,
  "notes": "Optional notes from parent"
}
```
Allows parents to agree/disagree to vaccination with optional notes.

## Manager Endpoints

### Get Campaign Assignments
```http
GET /api/vaccination/campaigns/{id}/assignments
Authorization: RequireManagerRole
```
Returns all nurse assignments for a campaign.

### Get Campaign Completion Report
```http
GET /api/vaccination/campaigns/{id}/completion-report
Authorization: RequireManagerRole
```
Returns comprehensive statistics about campaign completion including:
- Total students, consents sent/received/approved
- Vaccination completion rates
- Assigned nurses

### Update Campaign Status
```http
PUT /api/vaccination/campaigns/{id}/status
Authorization: RequireManagerRole
Content-Type: application/json

{
  "status": "InProgress"
}
```
Valid statuses: Planned, InProgress, Completed, Cancelled

### Send Campaign Notifications
```http
POST /api/vaccination/campaigns/{id}/send-notifications
Authorization: RequireManagerRole
```
Sends notifications to all parents about the vaccination campaign.

## Nurse Endpoints

### Get Nurse Assignments
```http
GET /api/vaccination/assignments/nurse/{nurseId}
Authorization: RequireNurseRole
```
Returns all campaigns assigned to a specific nurse.

### Get Campaign Students
```http
GET /api/vaccination/campaigns/{id}/students
Authorization: RequireNurseRole
```
Returns all students for a campaign with their consent status and vaccination records.

### Update Vaccination Record Status
```http
PUT /api/vaccination/record/{id}/status
Authorization: RequireNurseRole
Content-Type: application/json

{
  "status": "Done",
  "notes": "Optional notes"
}
```
Valid statuses: Pending, Done, Absent, Cancelled

## Notification Endpoints

### Get User Notifications
```http
GET /api/Notification/user/{userId}
Authorization: User must access their own notifications
```
Returns all notifications for a user.

### Mark Notification as Read
```http
PUT /api/Notification/mark-read/{notificationId}
Authorization: User must own the notification
```
Marks a notification as read.

## Data Transfer Objects (DTOs)

### VaccinationConsentResponse
```json
{
  "consentStatus": true,
  "notes": "Optional parent notes"
}
```

### CampaignCompletionReport
```json
{
  "campaignId": 1,
  "vaccineName": "COVID-19",
  "targetClass": "Class 5A",
  "scheduleDate": "2024-01-15",
  "status": "InProgress",
  "totalStudents": 30,
  "consentsSent": 30,
  "consentsReceived": 25,
  "consentsApproved": 23,
  "consentsRejected": 2,
  "vaccinationsCompleted": 20,
  "studentsAbsent": 3,
  "consentResponseRate": 83.33,
  "consentApprovalRate": 92.0,
  "completionRate": 86.95,
  "assignedNurses": [...]
}
```

### StudentConsentInfo
```json
{
  "studentId": 1,
  "studentName": "John Doe",
  "class": "Class 5A",
  "consentId": 10,
  "consentStatus": true,
  "consentDate": "2024-01-10",
  "consentNotes": "No allergies",
  "vaccinationStatus": "Done",
  "vaccinationDate": "2024-01-15"
}
```

## Business Rules

1. **Parent Access Control**: Parents can only access consent forms for their own children
2. **Campaign Status Validation**: Cannot respond to consents for completed or cancelled campaigns
3. **Date Validation**: Cannot respond to consents for past campaigns
4. **Nurse Authorization**: Nurses can only access their assigned campaigns
5. **Automatic Timestamps**: Consent and vaccination dates are automatically set when actions are performed

## Error Handling

All endpoints return appropriate HTTP status codes:
- `200 OK`: Successful operation
- `400 Bad Request`: Invalid input or business rule violation
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

Error responses include descriptive messages to help with troubleshooting.