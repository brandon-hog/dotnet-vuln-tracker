namespace Application.Interfaces;

public interface INvdService
{
    // Initializes the database with the NVD data
    Task InitializeDatabase();

    // Updates the database with data recently modified within 24 hours
    Task UpdateDatabase();
}