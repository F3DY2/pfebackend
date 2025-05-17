namespace pfebackend.DTOs
{
    public class UserUpdateDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public int? TotalNumberOfCars { get; set; }
        public int? TotalNumberOfBedrooms { get; set; }
        public int? TotalNumberOfFamilyMembers { get; set; }
        public int? TotalNumberOfFamilyMembersEmployed { get; set; }
    }
}
