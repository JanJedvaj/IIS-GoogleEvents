namespace IISGoogleEvents.Domain.Enums;

/// <summary>
/// Values are spaced so that a "minimum role" check can be expressed as >=,
/// and so new roles can be slotted between existing ones without a migration.
/// </summary>
public enum Roles
{
    User = 100,
    Admin = 200
}
