global using MovieTuple = (string title, int runLengthMinutes, string advertisingMesssage, string leads);
global using ShowingTuple = (int showingID, System.DateTime showingDateTime, decimal ticketPrice, int theaterRoom, string movieTitle);
global using DailyShowingTuple = (string MovieTitle, System.DateTime showTime);

global using PreferredCustomerTuple = (int preferredCustomerID, string name, string email, int ticketPoints, int concessionPoints);
global using SoldTicketTuple = (System.DateTime soldDateTime, int showingID, decimal revenueCharged, int preferredCustomerNum);

//concessions
global using ConcessionMenuTuple = (string itemName, string itemDescription, decimal price);
global using ConcessionSaleTuple = (System.DateTime soldDateTime, string itemName, int quantitySold, decimal revenueCollected, int preferredCustomerID);

// advertisements
global using AdvertisementTuple = (string name, string description, int lengthInSeconds, decimal chargePerPlay);
global using ScheduledAdsTuple = (int scheduleShowingID, string advertisementName);


using System;
using System.Data;
using System.Globalization;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Shared;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


internal class Program
{
  public static PreferredCustomerTuple? currentCustomer = null;
  private static void Main()
  {
    Shared.MovieTheater.ReadDataInFromAllFiles();
    MainMenu();
    Shared.MovieTheater.WriteDataToAllFiles();

  }
  public static void MainMenu()
  {
    while (true)
    {
      Console.Clear();
      Console.WriteLine("***Main Menu***");
      string menu = "1-Ticket Window\n" +
                      "2-Concession Stand\n" +
                      "3-Advertisement Controls\n" +
                      "4-Scheduling Controls\n" +
                      "5-Theaterwide Controls\n" +
                      "6-Save and Exit\n" +
                      "What would you like to do? ";
      int choice = getIntWillLoop(menu, 1, 6);
      if (choice == 1)//Ticket Window
      {
        TicketWindowMenu();
      }
      else if (choice == 2)//Concession Stand
      {
        ConcessionMenu();
      }
      else if (choice == 3)//Advertisment Controls
      {
        // AdvertisementMenu();
      }
      else if (choice == 4)//Scheduling Controls
      {
        SchedulingControlsMenu();
      }
      else if (choice == 5)//Theaterwide Controls
      {
        Console.Clear();
        DateOnly selectedDay = getDateOnlyWillLoop("Which day would you like to see reported");
        Console.WriteLine(MovieTheater.ConcessionReport5_ItemTotalsPerDay(selectedDay));
        PressKeyToContinue("Press any key to continue");
      }
      else if (choice == 6)// Save and Exit
      {
        return;
      }
    }
  }
  public static void ConcessionMenu()
  {
    while (true)
    {
      Console.Clear();
      Console.WriteLine("***Concession Window Menu***");
      string menu = "1-View Menu Items\n" +
                      "2-Purchase a Concession\n" +
                      "3-All Sales Report\n" +
                      "4-Revenue Per Day Report\n" +
                      "5-Item Sold for a Given Day\n" +
                      "6-Return to Main Menu\n" +
                      "What would you like to do?";
      int choice = getIntWillLoop(menu, 1, 6);
      if (choice == 1)//Print Menu
      {
        Console.Clear();
        Console.WriteLine("****Concessions Menu****");
        Console.WriteLine($"{"NAME",-30} {"DESCRIPTION",-30} {"PRICE",-10:C2}");
        foreach (var mi in MovieTheater.ConcessionMenuList)
        {
          Console.WriteLine($"{mi.itemName,-30} {mi.itemDescription,-30} {mi.price,-10:C2}");
        }
        Console.WriteLine("\nPress any key to continue.");
        Console.ReadKey(true);
      }
      if (choice == 2)//Purchase Concession Item
      {
        Console.Clear();
        Console.WriteLine("***Purchase Concessions***");
        // display items and walk user through purchase
        System.Console.WriteLine("Available concessions menu");
        Console.WriteLine($"{"#",-3}{"NAME",-30} {"DESCRIPTION",-30} {"PRICE",-10:C2}");
        for (int i = 0; i < MovieTheater.ConcessionMenuList.Count; i++)
        {
          System.Console.WriteLine($"{i + 1,-2} {MovieTheater.ConcessionMenuList[i].itemName,-30} {MovieTheater.ConcessionMenuList[i].itemDescription,-31}{MovieTheater.ConcessionMenuList[i].price,-10:C2}");

        }


        int toBuyID = getIntWillLoop("Enter the item # you wish to buy", 1, MovieTheater.ConcessionMenuList.Count);

        int toQuantitytoBuy = getIntWillLoop("How many do you want to buy?", 1, 999);
        //Calculate the bill 
        decimal billamt = MovieTheater.ConcessionMenuList[toBuyID - 1].price * toQuantitytoBuy;
        Console.WriteLine("Would you like to Login to your preferred customer accounts? (y/n)");
        // display list of movies that have showtimes, have user select one
        // display a list of showings for that movie, have user select one

        string loginChoice = Console.ReadLine();
        if (loginChoice == "y")
        {
          LoginPreferredCustomer();
        }
        else
        {
          currentCustomer = null;
        }
        string custname;
        //Ask for name
        if (currentCustomer == null)
        {
          System.Console.WriteLine("What is the customer's name?");
          custname = Console.ReadLine();
        }
        else
        {
          custname = currentCustomer.Value.name;
        }

        //Do the purchase

        MovieTheater.PurchaseMenuItem(custname,
           MovieTheater.ConcessionMenuList[toBuyID - 1].itemName, toQuantitytoBuy, false);

        //add the points if they're logged in
        if (currentCustomer != null)
        {
          MovieTheater.PreferredCustomerList.Remove(currentCustomer.Value);
          PreferredCustomerTuple newCustomer = currentCustomer.Value;
          newCustomer.concessionPoints = currentCustomer.Value.concessionPoints + 1;
          MovieTheater.PreferredCustomerList.Add(newCustomer);
        }

        System.Console.WriteLine($"Thank you for buying {toQuantitytoBuy} of {MovieTheater.ConcessionMenuList[toBuyID - 1].itemName} for {billamt} ");

        PressKeyToContinue("\nTransaction Complete.\nPress any key to continue.");
      }
      if (choice == 3)//Receipts from All Sales
      {
        Console.Clear();
        System.Console.WriteLine("***All Sales Report***");
        Console.WriteLine(MovieTheater.ConcessionReport3_AllReceipts());
        PressKeyToContinue("Hit any key to move on");
      }
      if (choice == 4)//Revenue Totals For All Days
      {
        Console.Clear();
        System.Console.WriteLine("***Revenue Report Per Day***");
        Console.WriteLine(MovieTheater.ConcessionReport4_RevenueTotalsForAllDays());
        PressKeyToContinue("Hit any key to move on");
      }
      if (choice == 5)//Display Item Revenue For A Given Day
      {
        Console.Clear();
        // Have the user input a date
        DateOnly selectedDay = getDateOnlyWillLoop("Which day would you like to see reported?");
        Console.WriteLine(MovieTheater.ConcessionReport5_ItemTotalsPerDay(selectedDay));

        PressKeyToContinue("Hit any key to move on");
      }
      if (choice == 6)//return to main menu
      {
        return;
      }
    }
  }

  public static DateOnly getDateOnlyWillLoop(string prompt)
  {
    System.Console.WriteLine(prompt);
    System.Console.WriteLine("Format like MM/DD/YYYY");
    try
    {
      string input = Console.ReadLine();
      return DateOnly.Parse(input);
    }
    catch
    {
      System.Console.WriteLine("That was invalid");
      return getDateOnlyWillLoop(prompt);
    }
    return new DateOnly();
  }
  public static DateTime getDateTimeWillLoop(string prompt)
  {
    System.Console.WriteLine(prompt);
    System.Console.WriteLine("Format like MM/DD/YYYY for the date and then HH:MM:SS (24 hour time only).");
    try
    {
      string input = Console.ReadLine();
      return DateTime.Parse(input);
    }
    catch
    {
      System.Console.WriteLine("That was invalid");
      return getDateTimeWillLoop(prompt);
    }
  }
  public static DateOnly getDateOnlyWillLoop(string prompt, List<DateOnly> dayOnlyOptions)
  {
    for (int i = 0; i < dayOnlyOptions.Count; i++)
    {
      System.Console.WriteLine($"{i + 1}; {dayOnlyOptions[i]}");
    }
    int choice = getIntWillLoop(prompt, 1, dayOnlyOptions.Count + 1);
    return dayOnlyOptions[choice - 1];
  }

  public static int getIntWillLoop(string prompt, int min = int.MinValue, int max = int.MaxValue)
  {
    while (true)
    {
      Console.WriteLine(prompt);
      int number;
      if (int.TryParse(Console.ReadLine(), out number))
      {
        if (number >= min && number <= max)
        {
          return number;
        }
      }
      Console.Write("Invalid. Please enter valid number: ");
    }
  }

  public static int getIdNoDupes(string prompt, int min, int max)
  {
    while (true)
    {
      int input = getIntWillLoop(prompt, min, max);
      bool isDuplicate = false;
      foreach (PreferredCustomerTuple customer in MovieTheater.PreferredCustomerList)
      {
        if (customer.preferredCustomerID == input)
        {
          Console.WriteLine("Invalid. Please enter an ID that hasn't been used yet.");
          isDuplicate = true;
          break;
        }
      }
      if (!isDuplicate)
      {
        return input;
      }
    }
  }

  static string GetValidEmail(string prompt)
  {
    string email;
    while (true)
    {
      Console.WriteLine(prompt);
      email = Console.ReadLine();
      if (IsValidEmail(email))
      {
        return email;
      }
      else
      {
        Console.WriteLine("Invalid email format. Please enter a valid email address.");
      }
    }
  }

  static bool IsValidEmail(string email)
  {
    string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    return Regex.IsMatch(email, pattern);
  }



  public static decimal getDecimalWillLoop(string prompt, int min, int max)
  {
    while (true)
    {
      System.Console.WriteLine(prompt);
      decimal number;
      if (decimal.TryParse(Console.ReadLine(), out number))
      {
        if (number >= min && number <= max)
        {
          return number;
        }
      }
    }
    //todo
  }

  public static bool GetBoolWillLoop(string prompt)
  {
    while (true)
    {
      Console.WriteLine(prompt);
      string input = Console.ReadLine().ToUpper();
      if (input == "YES" || input == "Y" || input.ToLower() == "true" || input.ToLower() == "t") return true;
      else if (input == "NO" || input == "N" || input.ToLower() == "false" || input.ToLower() == "f") return false;
      Console.Write("Invalid.  Please enter a valid True/False/Yes/No answer");
    }
  }


  public static void PressKeyToContinue(string prompt)
  {
    while (Console.KeyAvailable)
    {
      Console.ReadKey(true);
    }
    Console.WriteLine(prompt);
    Console.ReadKey(true);
  }

  public static void SchedulingControlsMenu()
  {
    while (true)
    {
      Console.Clear();
      Console.WriteLine("***Scheduling Controls***");
      string menu = "1-New Movie\n" +
                      "2-Create ShowingID\n" +
                      "3-View Regsitered Movies\n" +
                      "4-View Showtimes for Movies\n" +
                      "5-Return to Main Menu\n" +
                      "What would you like to do? ";
      int choice = getIntWillLoop(menu, 1, 5);
      if (choice == 1) //New Movie
      {
        Console.Clear();
        Console.WriteLine("***Register New Movie***");
        System.Console.WriteLine("Input movie title");
        string title = Console.ReadLine();

        int runLengthMinutes = getIntWillLoop("Input run length in minutes", 1, int.MaxValue);
        Console.WriteLine("Input advertising message");
        string advertisingMesssage = Console.ReadLine();
        System.Console.WriteLine("Input lead actors");
        string leads = Console.ReadLine();

        try
        {
          MovieTheater.NewMovieRegistration(title, runLengthMinutes, advertisingMesssage, leads);
        }
        catch (Exception e)
        {
          System.Console.WriteLine(e.Message);
          System.Console.WriteLine("Error while adding movie");
        }

        PressKeyToContinue("\nPress any key to continue.");
      }

      if (choice == 2) //Create ShowingID
      {
        Console.Clear();
        Console.WriteLine("***Register New Showing ID***");
        // get a new showing ID from the user
        int movieNumber = getIntWillLoop("Insert a number of the showing ID", 1, int.MaxValue);
        // have the user input the date and time of the showing
        DateTime dateTime = getDateTimeWillLoop("Insert the date and time of the showing.");
        // use getDecimalWillLoop to have the user input a ticket price
        decimal ticketPrice = getDecimalWillLoop("Insert the ticket price.", 0, int.MaxValue);
        // have the user select a theater room (implement getValidTheaterRoomWillLoop)
        int theatreid = getValidTheaterRoomWillLoop("Please enter the theatre ID.");
        System.Console.WriteLine("What is the name of the movie?");
        string movieTitle = Console.ReadLine();

        // use MovieTheater.CreateShowingID to create the showing
        //    - if MovieTheater.CreateShowingID throws an exception, let the user know something went wrong and keep going.
        try
        {
          System.Console.WriteLine("Attempting to create showing ID.");
          MovieTheater.CreateShowingID(movieNumber, dateTime, ticketPrice, theatreid, movieTitle);
        }
        catch
        {
          System.Console.WriteLine("Something went wrong, please try again.");

        }
        PressKeyToContinue("\nPress any key to continue.");
      }
      if (choice == 3)//return to main menu
      {
        System.Console.WriteLine("***Movie Report***");
        foreach (MovieTuple movie in MovieTheater.MovieList)
        {
          System.Console.WriteLine($"TITLE: {movie.title}\t{movie.runLengthMinutes}\t{movie.advertisingMesssage}");
          System.Console.WriteLine($"LEADS: {movie.leads}");
          System.Console.WriteLine("\n");
        }
        PressKeyToContinue("Press any key to continue.");
      }
      if (choice == 4)
      {
        Console.Clear();
        string movieTitle = getValidMovieTitleWillLoop("Which movie would you like to see showtimes for?");
        Console.Clear();
        string output = MovieTheater.MovieShowingReport(movieTitle);
        System.Console.WriteLine(output);
        PressKeyToContinue("\nPress any key to Continue");
      }
      if (choice == 5)
      {
        return;
      }
    }


  }

  public static string getValidMovieTitleWithShowingsWillLoop(string prompt)
  {
    List<string> movieTitlesWithShowings = new();
    foreach (ShowingTuple showing in MovieTheater.ScheduleList)
    {
      if (!movieTitlesWithShowings.Contains(showing.movieTitle))
      {
        movieTitlesWithShowings.Add(showing.movieTitle);
      }
    }
    System.Console.WriteLine(prompt);
    for (int i = 0; i < movieTitlesWithShowings.Count; i++)
    {
      System.Console.WriteLine($"{i + 1,4}: {movieTitlesWithShowings[i]}");
    }
    int userChoice = getIntWillLoop("Select a Movie", 1, movieTitlesWithShowings.Count);
    return movieTitlesWithShowings[userChoice - 1];
  }


  public static string getValidMovieTitleWillLoop(string prompt)
  {
    System.Console.WriteLine(prompt);
    for (int i = 0; i < MovieTheater.MovieList.Count; i++)
    {
      System.Console.WriteLine($"{i + 1,4}: {MovieTheater.MovieList[i].title}");
    }
    int userChoice = getIntWillLoop("Select a Movie", 1, MovieTheater.MovieList.Count);
    return MovieTheater.MovieList[userChoice - 1].title;
  }
  public static ShowingTuple getValidShowingWillLoop(string prompt, string movieTitle)
  {
    System.Console.WriteLine(prompt);
    List<ShowingTuple> showingsForMovie = new();
    foreach (ShowingTuple showing in MovieTheater.ScheduleList)
    {
      if (showing.movieTitle == movieTitle)
      {
        showingsForMovie.Add(showing);
      }
    }
    for (int i = 0; i < showingsForMovie.Count; i++)
    {
      System.Console.WriteLine($"{i + 1,4}:{showingsForMovie[i].showingDateTime}");
    }
    int choice = getIntWillLoop("Select a time", 1, showingsForMovie.Count);
    return showingsForMovie[choice - 1];
  }
  public static int getValidTheaterRoomWillLoop(string prompt)
  {
    System.Console.WriteLine(prompt);
    foreach (KeyValuePair<int, int> room in MovieTheater.TheaterRoomCapacity)
    {
      System.Console.WriteLine($"{room.Key,4}: capacity {room.Value}");

    }
    System.Console.WriteLine("Input a room number:");
    string userInputNumber = Console.ReadLine();
    try
    {
      int roomNumber = int.Parse(userInputNumber);

      if (MovieTheater.TheaterRoomCapacity.ContainsKey(roomNumber))
      {
        return roomNumber;
      }
      else
      {
        System.Console.WriteLine("That is not a valid room number");
        return getValidTheaterRoomWillLoop(prompt);
      }
    }
    catch
    {
      System.Console.WriteLine("Invalid number, try again");
      return getValidTheaterRoomWillLoop(prompt);
    }
  }
  // show the user a list of theater rooms
  // allow them to input a number to select one
  // return the ID of the theater room they selected

  public static void TicketWindowMenu()
  {
    while (true)
    {
      Console.Clear();
      Console.WriteLine("***Ticket Window Menu***");
      string menu = "1-Purchase Ticket\n" +
                      "2-Seat Availability\n" +
                      "3-Daily Movies & Showtime Report\n" +
                      "4-Preferred Customer Registration\n" +
                      "5-Preferred Customer Report\n" +
                      "6-Daily Ticket Sales Revenue Report\n" +
                      "7-Return to Main Menu\n" +
                      "What would you like to do? ";
      int choice = getIntWillLoop(menu, 1, 7);
      if (choice == 1) //Purchase Ticket
      {
        PurchaseTicket();
      }
      else if (choice == 2) //Seat Avalability
      {
        Console.Clear();
        Console.WriteLine("****Seat Avalibility****");

        string movieTitle = getValidMovieTitleWillLoop("Pick a Movie:");
        ShowingTuple showing = getValidShowingWillLoop("Pick a Showing", movieTitle);
        int ticketsAvailable = MovieTheater.HowManySeatsAreAvailableForShowing(showing.showingID);
        System.Console.WriteLine($"There are {ticketsAvailable} tickets available for {movieTitle}");

        PressKeyToContinue("Press any key to continue.");
      }
      else if (choice == 3) //Daily Movies & Showtime Report
      {
        Console.Clear();
        Console.WriteLine("****Daily Movies & Showtime Report****");

        // have user select day that has showtimes
        // display list of showings (with movie names) to user, have them select one
        // display movie details and lead actors to user
      }
      else if (choice == 4)//Preferred Customer Regestration
      {
        // todo when we implement preferred customer
        //allow a new preferred customer to be created and stored.
        Console.Clear();
        Console.WriteLine("****Register Preferred Customer****");
        int id = getIdNoDupes("Please enter id number:", 1, int.MaxValue); //MAKE SURE THEY DON"T CHOOSE EXISITNG

        Console.WriteLine("Please enter the name.");
        string name = Console.ReadLine();
        string email = GetValidEmail("Please enter the email.");
        PreferredCustomerTuple newPreferredCustomer = new()
        {
          preferredCustomerID = id,
          name = name,
          email = email
        };

        MovieTheater.PreferredCustomerList.Add(newPreferredCustomer);
        MovieTheater.WriteDataToAllFiles();
        PressKeyToContinue("Saved. Press any key to continue.");
      }
      else if (choice == 5)//Preferred Customer Report
      {
        Console.WriteLine("*** Preferred Customer Report ***");
        Console.WriteLine($"{"ID:",-5}{"Name:",-20}{"Email:"}");
        foreach (var customer in MovieTheater.PreferredCustomerList)
        {
          Console.WriteLine($"{customer.preferredCustomerID,-5}{customer.name,-20}{customer.email}");
        }
        PressKeyToContinue("Press any key to continue.");
      }
      else if (choice == 6)//Daily Ticket Sales Revenue Report
      {
        DateOnly choice5RevenueDate = getDateOnlyWillLoop("You are picking a date for the ticket sales revenue report.");
        string Report = MovieTheater.TicketReport5_TicketSalesRevenue(choice5RevenueDate);
        Console.WriteLine("Here are the sales for the date you entered");
        Console.WriteLine(Report);
        PressKeyToContinue("Press any key to Continue");

      }
      else if (choice == 7)//return to main menu
      {
        return;
      }
    }
  }
  public static void PurchaseTicket()
  {
    // display list of movies that have showtimes, have user select one
    string movieTitle = getValidMovieTitleWithShowingsWillLoop("Select a Movie.");
    // display a list of showings for that movie, have user select one
    ShowingTuple showing = getValidShowingWillLoop("Select a showing", movieTitle);

    int ticketsAvailable = MovieTheater.HowManySeatsAreAvailableForShowing(showing.showingID);
    System.Console.WriteLine($"There are {ticketsAvailable} tickets available for {movieTitle}");
    int ticketCount = getIntWillLoop("How many tickets would you like to buy?", 1, ticketsAvailable);
    decimal cost = ticketCount * showing.ticketPrice;
    Console.WriteLine("Would you like to use your preferred customer benefits? (y/n)");
    string choice = Console.ReadLine();
    if (choice == "y")
    {
      LoginPreferredCustomer();
    }
    else
    {
      currentCustomer = null;
    }

    System.Console.WriteLine($"That will cost {cost:$0.00}");
    MovieTheater.TicketPurchase(showing.showingID, cost);

    if (currentCustomer != null)
    {
      MovieTheater.PreferredCustomerList.Remove(currentCustomer.Value);
      PreferredCustomerTuple newCustomer = currentCustomer.Value;
      newCustomer.ticketPoints = currentCustomer.Value.ticketPoints + 1;
      MovieTheater.PreferredCustomerList.Add(newCustomer);
      MovieTheater.WriteDataToAllFiles();
    }


    PressKeyToContinue("Transaction recorded, press any key to continue");

  }


  public static void LoginPreferredCustomer()
  {
    Console.Clear();
    Console.WriteLine("**** Login ****");

    while (true)
    {
      int id = getIntWillLoop("Please enter your preferred customer ID.", 1, int.MaxValue);
      PreferredCustomerTuple? customer = MovieTheater.PreferredCustomerList.FirstOrDefault(c => c.preferredCustomerID == id);
      if (customer != null)
      {
        currentCustomer = MovieTheater.PreferredCustomerList.FirstOrDefault(c => c.preferredCustomerID == id);
        Console.WriteLine($"Welcome, {currentCustomer.Value.name}!");
        break;
      }
      else
      {
        Console.WriteLine("Customer ID not found.");
        bool tryAgain = GetBoolWillLoop("Would you like to try again? (yes/no)");
        if (!tryAgain)
        {
          return;
        }
      }
    }

  }
}