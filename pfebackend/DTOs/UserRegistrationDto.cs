namespace pfebackend.DTOs
{
    public class UserRegistrationDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? AgriculturalHouseHoldIndicator { get; set; }

        public int? TotalNumberOfFamilyMembers { get; set; }
        public int? TotalNumberOfFamilyMembersEmployed { get; set; }

    }
}
