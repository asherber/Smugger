using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smugger.Samples
{
    class UserSample
    {
        public static async Task WorkingWithUsers(SmugMugClient api)
        {
            //Get a given user
            User user = await api.GetUserAsync("justmarks");
            Console.WriteLine(user.Name);

            //Get the user's profile
            UserProfile userProfile = await api.GetUserProfileAsync(user);
            Console.WriteLine("{0} - Twitter:", userProfile.DisplayName, userProfile.Twitter);
        }
    }
}