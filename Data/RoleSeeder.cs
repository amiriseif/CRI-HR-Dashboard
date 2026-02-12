using Microsoft.AspNetCore.Identity;

public static class RoleSeeder
{
    public static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager)
    {
        String[] rolenames={"RH manager","Bureau Superieur"};
        foreach(var rolename in rolenames)
        {
            var roleExist = await roleManager.RoleExistsAsync(rolename);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(rolename));
            }
        }
    }

}