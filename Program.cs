using MySqlConnector;

string veza = Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
    ?? throw new Exception("Nedostaje MYSQL_CONNECTION.");

using (var db = new MySqlConnection(veza))
{
    db.Open();
    Console.WriteLine("Uspješno spojeno na MySQL!");

    string sql = "SELECT id, naziv, cijena FROM proizvodi";

    using var cmd = new MySqlCommand(sql, db);
    using var reader = cmd.ExecuteReader();

    Console.WriteLine("\n--- PROIZVODI IZ BAZE ---");

    while (reader.Read())
    {
        int id = reader.GetInt32("id");
        string naziv = reader.GetString("naziv");
        decimal cijena = reader.GetDecimal("cijena");

        Console.WriteLine($"{id}. {naziv} - {cijena} EUR");
    }
}
Console.WriteLine("=== MINI RESTORAN ===");




// Lista u koju spremamo narucene proizvode
List<string> stavke = new List<string>();

List<(int id, int kolicina, decimal cijena)> proizvodiNarudzbe = new();

decimal ukupno = 0;

while (true)
{
    Console.WriteLine("\n--- JELOVNIK ---");
    Console.WriteLine("1. Pizza - 10 EUR");
    Console.WriteLine("2. Burger - 8 EUR");
    Console.WriteLine("3. Sok - 3 EUR");
    Console.WriteLine("0. Zavrsi narudzbu");

    Console.Write("Odaberi proizvod: ");
    string? odabir = Console.ReadLine();

    if (odabir == "0")
    {
        break;
    }

    string naziv = "";
    decimal cijena = 0;

    if (odabir == "1")
    {
        naziv = "Pizza";
        cijena = 10;
    }
    else if (odabir == "2")
    {
        naziv = "Burger";
        cijena = 8;
    }
    else if (odabir == "3")
    {
        naziv = "Sok";
        cijena = 3;
    }
    else
    {
        Console.WriteLine("Nepoznat proizvod!");
        continue;
    }

    Console.Write("Unesi kolicinu: ");

    if (!int.TryParse(Console.ReadLine(), out int kolicina) || kolicina <= 0)
    {
        Console.WriteLine("Neispravna kolicina!");
        continue;
    }

    decimal iznos = cijena * kolicina;
    ukupno += iznos;

    // Spremamo proizvod u listu
    stavke.Add($"{kolicina} x {naziv} = {iznos} EUR");

    proizvodiNarudzbe.Add((int.Parse(odabir!), kolicina, cijena));

    Console.WriteLine("Proizvod dodan u narudzbu!");
}

if (stavke.Count > 0)
{
    using var db = new MySqlConnection(veza);
    db.Open();

    using var transakcija = db.BeginTransaction();

    try
    {
        using var cmd = new MySqlCommand(
            "INSERT INTO narudzbe (datum) VALUES (NOW())",
            db, transakcija);

        cmd.ExecuteNonQuery();

        long narudzbaId = cmd.LastInsertedId;

        foreach (var proizvod in proizvodiNarudzbe)
        {
            using var stavkaCmd = new MySqlCommand(
                @"INSERT INTO stavke_narudzbe
                  (narudzba_id, proizvod_id, kolicina, cijena)
                  VALUES (@narudzba, @proizvod, @kolicina, @cijena)",
                db, transakcija);

            stavkaCmd.Parameters.AddWithValue("@narudzba", narudzbaId);
            stavkaCmd.Parameters.AddWithValue("@proizvod", proizvod.id);
            stavkaCmd.Parameters.AddWithValue("@kolicina", proizvod.kolicina);
            stavkaCmd.Parameters.AddWithValue("@cijena", proizvod.cijena);

            stavkaCmd.ExecuteNonQuery();
        }

        transakcija.Commit();
        Console.WriteLine("Narudzba i proizvodi spremljeni u bazu!");
    }
    catch
    {
        transakcija.Rollback();
        throw;
    }
}

// Ispis racuna
Console.WriteLine("\n====== RACUN ======");

foreach (string stavka in stavke)
{
    Console.WriteLine(stavka);
}

Console.WriteLine("-------------------");
Console.WriteLine($"UKUPNO: {ukupno} EUR");
Console.WriteLine("===================");
Console.WriteLine("Hvala na posjeti!");