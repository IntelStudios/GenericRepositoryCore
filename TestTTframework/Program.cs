using GenericRepository.Contexts;
using System;
using System.Collections.Generic;
using TestTTframework.Models;
using TestTTframework.Repositories;

namespace TestTTframework
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                //GRMSSQLContext context = 
                //    new GRMSSQLContext(
                //        "Server=192.168.10.100;Database=TomTestEFcore;User Id=sa;Password=P@ssword2;");
                //var userRepo = new UserRepository(context);
                //List<User> users = userRepo.GRToList();
                
                //foreach (var item in users)
                //{
                //    Console.WriteLine("name: {0}, lastname: {1}", item.FirstName, item.LastName);
                //}

                //var roleRepo = new RoleRepository(context);

                //User u1 = new User() { FirstName = "Jan", LastName = "Adam" };
                //User u2 = new User() { FirstName = "Albert", LastName = "Novotny" };
                //userRepo.GREnqueueInsert(u1);
                //userRepo.GREnqueueInsert(u2);
               




                //Role r = roleRepo
                //    .GRGet(1);

                //Console.WriteLine("{0} + {1}", r.Name, r.Description);


                //Delete
                //repo.GREnqueueDelete<User>()
                //    .GRWhere(x => x.Id == 3)
                //    .GRExecute();


                //update with Exclude
                //repo.GREnqueueUpdate<User>
                //    (new User() { Id = 3, FirstName = "MyName", LastName = "LLLL" })
                //    .GRForceExclude(x => x.LastName)
                //    .GRExecute();


                //insert 
                //repo.GREnqueueInsert<User>(
                //    new User() { FirstName = "sdf", LastName = "aasf" })
                //    .GRExecute();
                

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Hello World!");
        }


        
    }
}
