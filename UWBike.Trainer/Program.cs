using Microsoft.ML;
using UWBike.Trainer;

Console.WriteLine("=== UWBike ML Model Trainer ===");
Console.WriteLine("Treinamento: Previsão de Tempo de Permanência de Motos no Pátio");
Console.WriteLine();

// 1. Inicializar o Contexto do ML.NET
var mlContext = new MLContext(seed: 0);

// 2. Preparar dados de treinamento sintéticos
Console.WriteLine("Preparando dados de treinamento...");
var trainingData = new List<MotoStayDurationData>
{
    // Segunda-feira - horário comercial - alta demanda
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 85, TaxaOcupacao = 85, DiaSemana = 1, HoraDoDia = 9, MediaPermanenciaHistorica = 48, TempoPermanencia = 52 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 90, TaxaOcupacao = 90, DiaSemana = 1, HoraDoDia = 10, MediaPermanenciaHistorica = 48, TempoPermanencia = 56 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 80, TaxaOcupacao = 80, DiaSemana = 1, HoraDoDia = 14, MediaPermanenciaHistorica = 48, TempoPermanencia = 50 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 75, TaxaOcupacao = 75, DiaSemana = 1, HoraDoDia = 17, MediaPermanenciaHistorica = 48, TempoPermanencia = 45 },
    
    // Segunda-feira - baixa ocupação - saída mais rápida
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 30, TaxaOcupacao = 30, DiaSemana = 1, HoraDoDia = 8, MediaPermanenciaHistorica = 48, TempoPermanencia = 28 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 25, TaxaOcupacao = 25, DiaSemana = 1, HoraDoDia = 20, MediaPermanenciaHistorica = 48, TempoPermanencia = 24 },
    
    // Terça-feira - padrão similar a segunda
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 82, TaxaOcupacao = 82, DiaSemana = 2, HoraDoDia = 9, MediaPermanenciaHistorica = 47, TempoPermanencia = 50 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 88, TaxaOcupacao = 88, DiaSemana = 2, HoraDoDia = 11, MediaPermanenciaHistorica = 47, TempoPermanencia = 54 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 35, TaxaOcupacao = 35, DiaSemana = 2, HoraDoDia = 19, MediaPermanenciaHistorica = 47, TempoPermanencia = 30 },
    
    // Quarta-feira - ocupação média
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 65, TaxaOcupacao = 65, DiaSemana = 3, HoraDoDia = 10, MediaPermanenciaHistorica = 42, TempoPermanencia = 44 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 70, TaxaOcupacao = 70, DiaSemana = 3, HoraDoDia = 13, MediaPermanenciaHistorica = 42, TempoPermanencia = 46 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 60, TaxaOcupacao = 60, DiaSemana = 3, HoraDoDia = 16, MediaPermanenciaHistorica = 42, TempoPermanencia = 40 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 45, TaxaOcupacao = 45, DiaSemana = 3, HoraDoDia = 18, MediaPermanenciaHistorica = 42, TempoPermanencia = 36 },
    
    // Quinta-feira - ocupação média
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 62, TaxaOcupacao = 62, DiaSemana = 4, HoraDoDia = 9, MediaPermanenciaHistorica = 40, TempoPermanencia = 42 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 68, TaxaOcupacao = 68, DiaSemana = 4, HoraDoDia = 12, MediaPermanenciaHistorica = 40, TempoPermanencia = 44 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 55, TaxaOcupacao = 55, DiaSemana = 4, HoraDoDia = 15, MediaPermanenciaHistorica = 40, TempoPermanencia = 38 },
    
    // Sexta-feira - saída mais rápida (fim de semana chegando)
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 45, TaxaOcupacao = 45, DiaSemana = 5, HoraDoDia = 10, MediaPermanenciaHistorica = 32, TempoPermanencia = 30 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 50, TaxaOcupacao = 50, DiaSemana = 5, HoraDoDia = 14, MediaPermanenciaHistorica = 32, TempoPermanencia = 34 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 35, TaxaOcupacao = 35, DiaSemana = 5, HoraDoDia = 17, MediaPermanenciaHistorica = 32, TempoPermanencia = 26 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 20, TaxaOcupacao = 20, DiaSemana = 5, HoraDoDia = 19, MediaPermanenciaHistorica = 32, TempoPermanencia = 18 },
    
    // Sábado - muito baixa ocupação, saída rápida
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 25, TaxaOcupacao = 25, DiaSemana = 6, HoraDoDia = 10, MediaPermanenciaHistorica = 20, TempoPermanencia = 16 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 20, TaxaOcupacao = 20, DiaSemana = 6, HoraDoDia = 14, MediaPermanenciaHistorica = 20, TempoPermanencia = 14 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 15, TaxaOcupacao = 15, DiaSemana = 6, HoraDoDia = 16, MediaPermanenciaHistorica = 20, TempoPermanencia = 12 },
    
    // Domingo - baixíssima ocupação
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 20, TaxaOcupacao = 20, DiaSemana = 0, HoraDoDia = 11, MediaPermanenciaHistorica = 18, TempoPermanencia = 14 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 15, TaxaOcupacao = 15, DiaSemana = 0, HoraDoDia = 15, MediaPermanenciaHistorica = 18, TempoPermanencia = 10 },
    new MotoStayDurationData { CapacidadePatio = 100, MotosNoPatio = 10, TaxaOcupacao = 10, DiaSemana = 0, HoraDoDia = 17, MediaPermanenciaHistorica = 18, TempoPermanencia = 8 },
    
    // Diferentes capacidades de pátio - Pequeno (50)
    new MotoStayDurationData { CapacidadePatio = 50, MotosNoPatio = 45, TaxaOcupacao = 90, DiaSemana = 1, HoraDoDia = 10, MediaPermanenciaHistorica = 55, TempoPermanencia = 60 },
    new MotoStayDurationData { CapacidadePatio = 50, MotosNoPatio = 40, TaxaOcupacao = 80, DiaSemana = 3, HoraDoDia = 12, MediaPermanenciaHistorica = 55, TempoPermanencia = 52 },
    new MotoStayDurationData { CapacidadePatio = 50, MotosNoPatio = 20, TaxaOcupacao = 40, DiaSemana = 5, HoraDoDia = 15, MediaPermanenciaHistorica = 55, TempoPermanencia = 32 },
    
    // Pátio Grande (200)
    new MotoStayDurationData { CapacidadePatio = 200, MotosNoPatio = 170, TaxaOcupacao = 85, DiaSemana = 1, HoraDoDia = 9, MediaPermanenciaHistorica = 45, TempoPermanencia = 48 },
    new MotoStayDurationData { CapacidadePatio = 200, MotosNoPatio = 140, TaxaOcupacao = 70, DiaSemana = 3, HoraDoDia = 13, MediaPermanenciaHistorica = 45, TempoPermanencia = 42 },
    new MotoStayDurationData { CapacidadePatio = 200, MotosNoPatio = 80, TaxaOcupacao = 40, DiaSemana = 5, HoraDoDia = 17, MediaPermanenciaHistorica = 45, TempoPermanencia = 28 },
    new MotoStayDurationData { CapacidadePatio = 200, MotosNoPatio = 50, TaxaOcupacao = 25, DiaSemana = 6, HoraDoDia = 14, MediaPermanenciaHistorica = 45, TempoPermanencia = 18 },
    
    // Pátio Médio (75)
    new MotoStayDurationData { CapacidadePatio = 75, MotosNoPatio = 65, TaxaOcupacao = 87, DiaSemana = 2, HoraDoDia = 11, MediaPermanenciaHistorica = 50, TempoPermanencia = 54 },
    new MotoStayDurationData { CapacidadePatio = 75, MotosNoPatio = 50, TaxaOcupacao = 67, DiaSemana = 4, HoraDoDia = 14, MediaPermanenciaHistorica = 50, TempoPermanencia = 44 },
    new MotoStayDurationData { CapacidadePatio = 75, MotosNoPatio = 30, TaxaOcupacao = 40, DiaSemana = 5, HoraDoDia = 16, MediaPermanenciaHistorica = 50, TempoPermanencia = 30 },
};

Console.WriteLine($"Total de amostras de treinamento: {trainingData.Count}");

// 3. Carregar os dados
var dataView = mlContext.Data.LoadFromEnumerable(trainingData);

// 4. Definir o Pipeline de Treino
Console.WriteLine("Configurando pipeline de treinamento...");
var pipeline = mlContext.Transforms.Concatenate("Features",
        nameof(MotoStayDurationData.CapacidadePatio),
        nameof(MotoStayDurationData.MotosNoPatio),
        nameof(MotoStayDurationData.TaxaOcupacao),
        nameof(MotoStayDurationData.DiaSemana),
        nameof(MotoStayDurationData.HoraDoDia),
        nameof(MotoStayDurationData.MediaPermanenciaHistorica))
    .Append(mlContext.Regression.Trainers.FastTree(
        labelColumnName: "Label",
        numberOfLeaves: 20,
        numberOfTrees: 100,
        minimumExampleCountPerLeaf: 2));

// 5. Treinar o modelo
Console.WriteLine("Treinando o modelo...");
var model = pipeline.Fit(dataView);
Console.WriteLine("✓ Modelo treinado com sucesso!");

// 6. Salvar o modelo em um ficheiro .zip
var modelPath = "uwbike-ml-model.zip";
mlContext.Model.Save(model, dataView.Schema, modelPath);

Console.WriteLine();
Console.WriteLine($"✓ Modelo salvo como '{modelPath}'");
Console.WriteLine();
Console.WriteLine("PRÓXIMOS PASSOS:");
Console.WriteLine($"1. Copie o arquivo '{modelPath}' para a raiz do projeto UWBike");
Console.WriteLine("2. O MLPredictionService irá carregar automaticamente este modelo");
Console.WriteLine();

// 7. Fazer uma previsão de teste
Console.WriteLine("=== Teste de Previsão ===");
var predictionEngine = mlContext.Model.CreatePredictionEngine<MotoStayDurationData, MotoStayDurationPrediction>(model);

var testData = new MotoStayDurationData
{
    CapacidadePatio = 100,
    MotosNoPatio = 85,
    TaxaOcupacao = 85,
    DiaSemana = 1, // Segunda-feira
    HoraDoDia = 10,
    MediaPermanenciaHistorica = 48
};

var prediction = predictionEngine.Predict(testData);

Console.WriteLine($"Dados de teste:");
Console.WriteLine($"  - Capacidade do Pátio: {testData.CapacidadePatio}");
Console.WriteLine($"  - Motos no Pátio: {testData.MotosNoPatio}");
Console.WriteLine($"  - Taxa de Ocupação: {testData.TaxaOcupacao}%");
Console.WriteLine($"  - Dia da Semana: {testData.DiaSemana} (Segunda-feira)");
Console.WriteLine($"  - Hora do Dia: {testData.HoraDoDia}h");
Console.WriteLine($"  - Média Histórica: {testData.MediaPermanenciaHistorica}h");
Console.WriteLine();
Console.WriteLine($"⏱️  Tempo de Permanência Previsto: {prediction.TempoPermanenciaPrevisto:F2} horas");
Console.WriteLine($"   (aproximadamente {(int)(prediction.TempoPermanenciaPrevisto / 24)} dias e {(int)(prediction.TempoPermanenciaPrevisto % 24)} horas)");
Console.WriteLine();
