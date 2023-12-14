using Bingoo.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbelt.Blazor.SpeechSynthesis;

namespace Bingoo
{
    public partial class BingooStart
    {
        private int currentCount = 0;

        private string numCaselle = "", price = "";
        private string? registraCartellaId, registraCasellaNumero1, registraCasellaNumero2, registraCasellaNumero3, registraCasellaNumero4, registraCasellaNumero5,
        registraCasellaNumero6, registraCasellaNumero7, registraCasellaNumero8, registraCasellaNumero9, registraCasellaNumero10,
        registraCasellaNumero11, registraCasellaNumero12, registraCasellaNumero13, registraCasellaNumero14, registraCasellaNumero15;

        private string? verificaCartellaId;
        private string? visualizzaCartellaId;

        private readonly List<(int, int)> rangeColumns = new()
        {
            (1, 9),
            (10, 19),
            (20, 29),
            (30, 39),
            (40, 49),
            (50, 69),
            (60, 69),
            (70, 79),
            (80, 90)
        };
        private readonly List<int> allNumbers = new();
        private readonly List<int> numberExtracted = new() { };
        Random random = new();
        SpeechSynthesisVoice? _italianVoice;

        private bool IsStarted { get; set; } = false;
        private bool IsPaused { get; set; } = false;

        private bool HasPlayerWinCinquina { get; set; } = false;
        private bool HasPlayerWinBingoo { get; set; } = false;
        private bool HasWinCinquina
        {
            get
            {
                if (string.IsNullOrEmpty(verificaCartellaId))
                {
                    return false;
                }

                var cartellaToVerify = this.Cartelle?.FirstOrDefault(cartella => cartella.Id == verificaCartellaId);
                if (cartellaToVerify is not null)
                {
                    //verifico cinquina
                    bool cinquina = cartellaToVerify.FirstRow.All(number => IsExtracted(Convert.ToInt32(number))) ||
                        cartellaToVerify.SecondRow.All(number => IsExtracted(Convert.ToInt32(number))) ||
                        cartellaToVerify.ThirdRow.All(number => IsExtracted(Convert.ToInt32(number)));
                    return cinquina;
                }

                return false;
            }
        }

        private bool HasWinBingoo
        {
            get
            {
                if (string.IsNullOrEmpty(verificaCartellaId))
                {
                    return false;
                }

                var cartellaToVerify = this.Cartelle?.FirstOrDefault(cartella => cartella.Id == verificaCartellaId);
                if (cartellaToVerify is not null)
                {
                    //verifico bingo
                    bool bingoo = cartellaToVerify.FirstRow.All(number => IsExtracted(Convert.ToInt32(number))) &&
                    cartellaToVerify.SecondRow.All(number => IsExtracted(Convert.ToInt32(number))) &&
                    cartellaToVerify.ThirdRow.All(number => IsExtracted(Convert.ToInt32(number)));
                    return bingoo;

                }

                return false;
            }
        }
        private bool SpeakInProgress { get; set; } = false;
        private int Velocity { get; set; } = 3;
        private string Errors { get; set; }
        private int? LastNumberExtracted => this.numberExtracted?.TakeLast(1)?.FirstOrDefault();
        private List<Cartella> Cartelle { get; set; }
        [Inject] private SpeechSynthesis SpeechSynthesis { get; set; }

        private List<int> Last4ExtractedNumbers => (numberExtracted?.Any() ?? false) ? numberExtracted.SkipLast(1).TakeLast(4).Reverse().ToList() : new List<int>();
        private int TotExtractedNumbers => numberExtracted?.Count ?? 0;
        private bool SpeechAvailable => this._italianVoice is not null;
        private bool NoErrors => string.IsNullOrEmpty(this.Errors);
        private int TotCartelle => this.Cartelle?.Count ?? 0;
        private string GameVelocity => Velocity switch
        {
            1 => "Super Rapido",
            2 => "Rapido",
            3 => "Normale",
            4 => "Lento",
            5 => "Super Lento",
            _ => "Normale"
        };

        void SpeakStarted(object? sender, EventArgs args) => this.SpeakInProgress = true;
        void SpeakEnded(object? sender, EventArgs args) => this.SpeakInProgress = false;
        private bool IsExtracted(int number) => numberExtracted.Contains(number);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await PrepareAsync();
                await LoadTextToSpeechIfNeededAsync();
                await LoadCartelleAsync();
            }
        }
        private decimal GetPremioCinquina()
        {
            _ = int.TryParse(numCaselle, out var numCaselleToInt);
            decimal.TryParse(price.Replace(".", ","), NumberStyles.Float, new CultureInfo("it-IT"), out var priceToDecimal);

            if (numCaselleToInt == 0 || priceToDecimal == 0)
                return 0;

            return Math.Round((numCaselleToInt * priceToDecimal) * 20 / 100, 2);
        }

        private decimal GetPremioBingoo()
        {
            _ = int.TryParse(numCaselle, out var numCaselleToInt);
            decimal.TryParse(price.Replace(".", ","), NumberStyles.Float, new CultureInfo("it-IT"), out var priceToDecimal);

            if (numCaselleToInt == 0 || priceToDecimal == 0)
                return 0;

            return Math.Round((numCaselleToInt * priceToDecimal) * 80 / 100, 2);
        }

        private async Task PrepareAsync()
        {
            this.random = new();
            this.numberExtracted.Clear();
            this.allNumbers.Clear();
            LoadAllNumbers();
            await LoadTextToSpeechIfNeededAsync();

            this.IsStarted = this.IsPaused = false;
            this.HasPlayerWinCinquina = this.HasPlayerWinBingoo = false;
            await InvokeAsync(StateHasChanged);

            void LoadAllNumbers()
            {
                //for (int i = 1; i < 91; i++) allNumbers.Add(i);
                List<int> temporaryNumbers = new();
                for (int i = 1; i < 91; i++) temporaryNumbers.Add(i);

                Random random = new();

                for (int i = 1; i < 91; i++)
                {
                    int numeroCasuale = temporaryNumbers[random.Next(0, temporaryNumbers.Count)];
                    allNumbers.Add(numeroCasuale);
                    temporaryNumbers.Remove(numeroCasuale);
                }
            }
        }

        async Task LoadTextToSpeechIfNeededAsync()
        {
            if (this._italianVoice is not null)
                return;

            var allVoices = (await this.SpeechSynthesis.GetVoicesAsync())?.ToList();
            var italianVoice = allVoices?.FirstOrDefault(voice => voice.Lang == "it-IT");
            if (italianVoice is null)
                return;

            this._italianVoice = italianVoice;
            this.SpeechSynthesis.UtteranceStarted += SpeakStarted;
            this.SpeechSynthesis.UtteranceEnded += SpeakEnded;
            StateHasChanged();
        }

        private async Task StartAsync()
        {
            this.Errors = string.Empty;
            if (string.IsNullOrEmpty(this.numCaselle) || string.IsNullOrEmpty(this.price))
            {
                this.Errors = "Prima di procedere, è necessario indicare le caselle vendute e il relativo prezzo unitario.";
                StateHasChanged();
                return;
            }

            await PrepareAsync();
            IsStarted = true;
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (SpeakInProgress)
                        continue;

                    if (IsPaused)
                        continue;

                    if (allNumbers.Count == 0 || !IsStarted)
                    {
                        Stop();
                        break;
                    }

                    int numberIndex = random.Next(0, allNumbers.Count);
                    int randomNumber = allNumbers[numberIndex];
                    if (numberExtracted.Contains(randomNumber))
                        continue;

                    numberExtracted.Add(randomNumber);
                    allNumbers.Remove(randomNumber);
                    await SpeakAsync(randomNumber);

                    await InvokeAsync(() => StateHasChanged());
                    Thread.Sleep(Velocity * 1000);
                }
            });

            async Task SpeakAsync(int number)
            {
                var message = number.ToString();
                if ((message.StartsWith("6") || message.StartsWith("7")) && message.Length == 2)
                    message += Environment.NewLine + message.ElementAt(0).ToString() + " " + message.ElementAt(1).ToString();

                await this.SpeechSynthesis.SpeakAsync(new SpeechSynthesisUtterance()
                {
                    Text = message,
                    Voice = this._italianVoice
                }); // 👈 Speak!
            }
        }

        private void ManageCartelle()
        {
            if (IsStarted)
            {
                IsPaused = true;
                this.Errors = string.Empty;
            }
            StateHasChanged();
        }

        private void Pause()
        {
            IsPaused = !IsPaused;
            this.Errors = string.Empty;
            StateHasChanged();
        }

        private async void Stop()
        {
            this.numCaselle = "";
            this.price = "";
            await this.PrepareAsync();
        }

        private async Task SaveCartellaAsync()
        {
            this.Errors = string.Empty;
            if (Cartelle?.Any(cartella => cartella.Id == registraCartellaId) ?? false)
            {
                this.Errors = "L'id della cartella che si sta registrando esiste già. Prova un altro id.";
                StateHasChanged();
                return;
            }

            var cartella = new Cartella()
            {
                Id = registraCartellaId,
                FirstRow = new()
                {
                    registraCasellaNumero1,
                    registraCasellaNumero2,
                    registraCasellaNumero3,
                    registraCasellaNumero4,
                    registraCasellaNumero5,
                },
                SecondRow = new()
                {
                    registraCasellaNumero6,
                    registraCasellaNumero7,
                    registraCasellaNumero8,
                    registraCasellaNumero9,
                    registraCasellaNumero10,
                },
                ThirdRow = new()
                {
                    registraCasellaNumero11,
                    registraCasellaNumero12,
                    registraCasellaNumero13,
                    registraCasellaNumero14,
                    registraCasellaNumero15,
                }
            };

            try
            {
                cartella.ValidateOrThrowException();
            }
            catch (Exception ex)
            {
                this.Errors = ex.Message;
                StateHasChanged();
                return;
            }

            (Cartelle ??= new()).Add(cartella);
            await SaveCartelleAsync();

            registraCartellaId = registraCasellaNumero1 = registraCasellaNumero2 = registraCasellaNumero3 = registraCasellaNumero4 = registraCasellaNumero5 =
            registraCasellaNumero6 = registraCasellaNumero7 = registraCasellaNumero8 = registraCasellaNumero9 = registraCasellaNumero10 =
            registraCasellaNumero11 = registraCasellaNumero12 = registraCasellaNumero13 = registraCasellaNumero14 = registraCasellaNumero15 = string.Empty;
            StateHasChanged();
        }

        private async Task SaveCartelleAsync()
        {
            if (!File.Exists("cartelle.json")) File.Create("cartelle.json");

            await File.WriteAllTextAsync("cartelle.json", JsonSerializer.Serialize(Cartelle));
        }

        private async Task LoadCartelleAsync()
        {
            if (!File.Exists("cartelle.json")) return;

            var cartelleContent = await File.ReadAllTextAsync("cartelle.json");
            if (!string.IsNullOrEmpty(cartelleContent))
            {
                try
                {
                    this.Cartelle = JsonSerializer.Deserialize<List<Cartella>>(cartelleContent);
                }
                finally
                {
                    StateHasChanged();
                }
            }
        }
    }
}
