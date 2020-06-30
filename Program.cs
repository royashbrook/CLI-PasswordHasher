using System;

namespace h
{
    class Program
    {
        static void Main(string[] args)
        {
            string result = "Syntax: hash <password> or hash <password> <hash>";
            if (args.Length==1 || args.Length==2){
                var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher();
                if(args.Length == 1)
                    result = passwordHasher.HashPassword(args[0]);
                if(args.Length == 2)
                    result = (passwordHasher.VerifyHashedPassword(args[1],args[0]).ToString() == "Success").ToString();
            }
            Console.WriteLine(result);
        }
    }
}
