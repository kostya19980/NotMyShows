using NotMyShows.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NotMyShows.Services
{
    public class SVDPP
    {
        private static int Factors = 5;         // The number of latent factors
        private static double TS = 0.025;       // The training speed 
        private static double L1 = 0.0005;      // Regularization coefficient lambda1
        private static double L2 = 0.0025;      // Regularization coefficient lambda2
        private static double EPS = 0.00001;    // Error precision accuracy coefficient
        private static double Threshold = 0.01; // Threshold coefficient
        private static List<double> BS_User = new List<double>(); // Declaring the vector of users baseline predictors
        private static List<double> BS_Item = new List<double>(); // Declaring the vector of items baseline predictors
        private static List<List<double>> MF_User = new List<List<double>>(); // Declaring the matrix of user's latent factors
        private static List<List<double>> MF_Item = new List<List<double>>(); // Declaring the matrix of item's latent factors
        private static List<UserRaitings> MatrixUI = new List<UserRaitings>(); // Declaring the matrix of ratings
        public SVDPP()
        {

        }
        static void Initialize()
        {
            // Constructing the matrix of user's latent factors by iteratively
            // appending the rows being constructed to the list of rows MF_UserRow
            for (int User = 0; User < MatrixUI.Count(); User++)
            {
                // Declare a list of items MF_UserRow rated by the current user
                List<double> MF_UserRow = new List<double>();
                // Add the set of elements equal to 0 to the list of items MF_UserRow.
                // The number of elements being added is stored in Factors variable
                MF_UserRow.AddRange(Enumerable.Repeat(0.00, Factors));
                // Append the current row MF_UserRow to the matrix of factors MF_User
                MF_User.Insert(User, MF_UserRow);
            }

            // Constructing the matrix of item's latent factors by iteratively
            // appending the rows being constructed to the list of rows MF_ItemRow
            for (int Item = 0; Item < MatrixUI.ElementAt(0).Raitings.Count(); Item++)
            {
                // Declare a list of items MF_ItemRow rated by the current item
                List<double> MF_ItemRow = new List<double>();
                // Add the set of elements equal to 0 to the list of items MF_ItemRow
                // The number of elements being added is stored in Factors variable
                MF_ItemRow.AddRange(Enumerable.Repeat(0.00, Factors));
                // Append the current row MF_ItemRow to the matrix of factors MF_Item
                MF_Item.Insert(Item, MF_ItemRow);
            }

            // Intializing the first elements of the matrices of user's 
            // and item's factors with values 0.1 and 0.05
            MF_User[0][0] = 0.1; MF_Item[0][0] = MF_User[0][0] / 2;

            // Construct the vector of users baseline predictors by 
            // appending the set of elements equal to 0.The number of elements being 
            // appended is equal to the actual number of rows in the matrix of ratings
            BS_User.AddRange(Enumerable.Repeat(0.00, MatrixUI.Count()));
            // Construct the vector of items baseline predictors by appending
            // the set of elements equal to 0. The number of elements appended 
            // is equal to the actual number of rows in the matrix of ratings
            BS_Item.AddRange(Enumerable.Repeat(0.00, MatrixUI.ElementAt(0).Raitings.Count()));
        }
        public static double GetProduct(List<double> VF_User, List<double> VF_Item)
        {
            // Initialize the variable that is used to 
            // store the inner product of two factorization vectors
            double Product = 0.00;
            // Iterating through the two factorization vectors
            for (int Index = 0; Index < Factors; Index++)
            {
                // Compute the value of product of the two components 
                // of those vectors having the same value of index and 
                // add this value to the value of the variable Product
                Product += VF_User[Index] * VF_Item[Index];
            }

            return Product;
        }
        public static double GetAverageRating(List<UserRaitings> Matrix)
        {
            // Initialize the variables Sum and Count to store the values of
            // sum of existing ratings in matrix of ratings and the count of
            // existing ratings respectively
            double Sum = 0; int Count = 0;
            // Iterating through the matrix of ratings
            for (int User = 0; User < Matrix.Count(); User++)
            {
                for (int Item = 0; Item < Matrix[User].Raitings.Count(); Item++)
                {
                    // For each rating performing a check if the current rating is unknown
                    if (Matrix[User].Raitings[Item].Raiting > 0)
                    {
                        // If not, add the value of the current rating to the value of variable Sum
                        Sum = Sum + Matrix[User].Raitings[Item].Raiting;
                        // Increment the loop counter variable of existing ratings by 1
                        Count = Count + 1;
                    }
                }
            }

            // Compute and return the value of average 
            // rating for the entire domain of existing ratings
            return Sum / Count;
        }
        static void Learn()
        {
            // Initializing the iterations loop counter variable
            int Iterations = 0;
            // Initializing the RMSE and RMSE_New variables to store
            // current and previous values of RMSE
            double RMSE = 0.00, RMSE_New = 1.00;
            // Computing the average rating for the entire domain of rated items
            double AvgRating = GetAverageRating(MatrixUI);
            // Iterating the process of the ratings prediction model update until
            // the value of difference between the current and previous value of RMSE
            // is greater than the value of error precision accuracy EPS (e.g. the learning
            // process has converged).
            while (Math.Abs(RMSE - RMSE_New) > EPS)
            {
                // Assign the previously obtained value of RMSE to the RMSE variable
                // Assign the variable RMSE_New equal to 0
                RMSE = RMSE_New; RMSE_New = 0;
                // Iterate through the matrix of ratings and for each existing rating compute
                // the error value and perform the stochastic gradient descent to update 
                // the main parameters of the ratings prediction model for the current user and item
                for (int User = 0; User < MatrixUI.Count(); User++)
                {
                    for (int Item = 0; Item < MatrixUI.ElementAt(0).Raitings.Count(); Item++)
                        // Perform a check if the current rating in the matrix of ratings is unknown.
                        // If not, perform the following steps to adjust the values of baseline
                        // predictors and factorization vectors
                        if (MatrixUI[User].Raitings.ElementAt(Item).Raiting > 0)
                        {
                            // Compute the value of estimated rating using formula (2)
                            double Rating = AvgRating + BS_User[User] +
                                BS_Item[Item] + GetProduct(MF_User[User], MF_Item[Item]);

                            // Compute the error value as the difference between the existing and estimated ratings
                            double Error = MatrixUI[User].Raitings.ElementAt(Item).Raiting - Rating;

                            // Add the value of error square to the current value of RMSE
                            RMSE_New = RMSE_New + Math.Pow(Error, 2);

                            // Update the value of average rating for the entire domain of ratings
                            // by performing stochastic gradient descent using formulas (7.1-5)
                            AvgRating = AvgRating + TS * (Error - L1 * AvgRating);
                            // Update the value of baseline predictor of the current user
                            // by performing stochastic gradient descent using formulas (7.1-5)
                            BS_User[User] = BS_User[User] + TS * (Error - L1 * BS_User[User]);
                            // Update the value of baseline predictor of the current item 
                            // by performing stochastic gradient descent using formulas (7.1-5)
                            BS_Item[Item] = BS_Item[Item] + TS * (Error - L1 * BS_Item[Item]);

                            // Update each component of the factorization vector for the current user and item
                            for (int Factor = 0; Factor < Factors; Factor++)
                            {
                                // Adjust the value of the current component of the user's factorization vector 
                                // by performing stochastic gradient descent using formulas (7.1-5)
                                MF_User[User][Factor] += TS * (Error * MF_Item[Item][Factor] + L2 * MF_User[User][Factor]);
                                // Adjust the value of the current component of the item's factorization vector 
                                // by performing stochastic gradient descent using formulas (7.1-5)
                                MF_Item[Item][Factor] += TS * (Error * MF_User[User][Factor] + L2 * MF_Item[Item][Factor]);
                            }
                        }
                }

                // Compute the current value of RMSE (root means square error)
                RMSE_New = Math.Sqrt(RMSE_New / (MatrixUI.Count() * MatrixUI.ElementAt(0).Raitings.Count()));

                // Performing a check if the difference between the values 
                // of current and previous values of RMSE exceeds the given threshold
                if (RMSE_New > RMSE - Threshold)
                {
                    // If so, reduce the values of training speed and threshold 
                    // by multiplying each value by the value of specific coefficients
                    TS *= 0.66; Threshold *= 0.5;
                }
                Iterations++; // Increment the iterations loop counter variable
                Debug.WriteLine("RMSE_Current: " + RMSE_New + " Iterations: " + Iterations);
            }
        }

        public static void Predict()
        {
            // Computing the average rating for the entire domain of rated items
            double AvgRating = GetAverageRating(MatrixUI);
            // Iterating through the MatrixUI matrix of ratings
            for (int User = 0; User < MatrixUI.Count(); User++)
            {
                for (int Item = 0; Item < MatrixUI.ElementAt(0).Raitings.Count(); Item++)
                {
                    // For each rating given to the current item by the current user 
                    // we're performing a check if the current item is unknown
                    if (MatrixUI[User].Raitings.ElementAt(Item).Raiting == 0)
                    {
                        // If so, compute the rating for the current 
                        // unrated item used baseline estimate formula (2)
                        MatrixUI[User].Raitings[Item].Raiting = AvgRating + BS_User[User] +
                                BS_Item[Item] + GetProduct(MF_User[User], MF_Item[Item]);
                    }
                }
            }
        }
        //public static void Test(List<UserRaitings> test)
        //{
        //    List<UserRaitings> test1 = new List<UserRaitings>();
        //    foreach(var user in test)
        //    {
        //        UserRaitings userRaitings = new UserRaitings
        //        {
        //            UserProfileId = user.UserProfileId,
        //            Raitings = new List<SeriesRaiting>()
        //        };
        //        foreach (var raiting in user.Raitings)
        //        {
        //            if(raiting.Raiting > 0 && user.Raitings.IndexOf(raiting)%2 == 0)
        //            {
        //                SeriesRaiting seriesRaiting = new SeriesRaiting
        //                {
        //                    SeriesId = raiting.SeriesId,
        //                    Raiting = raiting.Raiting
        //                };
        //                userRaitings.Raitings.Add(seriesRaiting);
        //                raiting.Raiting = 0;
        //            }
        //        }
        //        test1.Add(userRaitings);
        //    }
        //    double AvgRating = GetAverageRating(test);
        //    double RMSE = 0;
        //    for (int User = 0; User < test1.Count(); User++)
        //    {
        //        for (int Item = 0; Item < test1.ElementAt(0).Raitings.Count(); Item++)
        //        {
        //            int itemIndex = 
        //            double Rating = AvgRating + BS_User[User] +
        //                BS_Item[Item] + GetProduct(MF_User[User], MF_Item[Item]);
        //            double Error = test1[User].Raitings.ElementAt(Item).Raiting - Rating;
        //            RMSE = RMSE + Math.Pow(Error, 2);
        //        }
        //    }
        //    RMSE = Math.Sqrt(RMSE / (test.Count() * test.ElementAt(0).Raitings.Count()));
        //    Debug.WriteLine("Test RMSE: " + RMSE);
        //}
        public List<UserRaitings> Start(List<UserRaitings> raitings)
        {
            //List<UserRaitings> learn = new List<UserRaitings>();
            //List<UserRaitings> test = new List<UserRaitings>();
            //int learnCount = (2 * raitings[0].Raitings.Count) / 3;
            //int testCount = raitings[0].Raitings.Count - learnCount;
            //foreach (var item in raitings)
            //{
            //    UserRaitings learnRaitings = new UserRaitings
            //    {
            //        UserProfileId = item.UserProfileId,
            //        Raitings = item.Raitings.Take(learnCount).ToList()
            //    };
            //    learn.Add(learnRaitings);
            //    UserRaitings testRaitings = new UserRaitings
            //    {
            //        UserProfileId = item.UserProfileId,
            //        Raitings = item.Raitings.Skip(learnCount).Take(testCount).ToList()
            //    };
            //    test.Add(testRaitings);
            //}
            MatrixUI = raitings;
            Initialize(); // Initializing the ratings prediction model
            Learn();
            Predict();    // Predicting atings for the unrated items
            return MatrixUI;
        }
    }
}
