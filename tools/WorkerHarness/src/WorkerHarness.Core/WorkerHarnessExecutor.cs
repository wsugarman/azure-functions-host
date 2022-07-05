﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WorkerHarness.Core.WorkerProcess;

namespace WorkerHarness.Core
{
    public class DefaultWorkerHarnessExecutor : IWorkerHarnessExecutor
    {
        private readonly IWorkerProcessBuilder _workerProcessBuilder;
        private readonly IScenarioParser _scenarioParser;
        private readonly ILogger<DefaultWorkerHarnessExecutor> _logger;
        private readonly HarnessOptions _harnessOptions;

        public DefaultWorkerHarnessExecutor(
            IWorkerProcessBuilder workerProcessBuilder,
            IScenarioParser scenarioParser,
            ILogger<DefaultWorkerHarnessExecutor> logger,
            IOptions<HarnessOptions> harnessOptions)
        {
            _workerProcessBuilder = workerProcessBuilder;
            _scenarioParser = scenarioParser;
            _logger = logger;
            _harnessOptions = harnessOptions.Value;
        }

        public async Task<bool> Start()
        {

            Process myProcess = _workerProcessBuilder.Build(_harnessOptions.LanguageExecutable!, _harnessOptions.WorkerExecutable!, _harnessOptions.WorkerDirectory!);

            string scenarioFile = _harnessOptions.ScenarioFile!;

            Scenario scenario = _scenarioParser.Parse(scenarioFile);

            try
            {
                myProcess.Start();

                foreach (IAction action in scenario.Actions)
                {
                    ActionResult actionResult = await action.ExecuteAsync();

                    ShowActionResult(actionResult);
                    
                }

                myProcess.Kill();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n{ex}");

                return false;
            }
        }

        private void ShowActionResult(ActionResult actionResult)
        {
            switch (actionResult.Status)
            {
                case StatusCode.Success:
                    _logger.LogInformation(actionResult.Message);
                    break;

                case StatusCode.Failure:
                    _logger.LogError(actionResult.Message);

                    IEnumerable<string> messages = _harnessOptions.DisplayVerboseError ? actionResult.VerboseErrorMessages : actionResult.ErrorMessages;
                    foreach (string message in messages)
                    {
                        _logger.LogError(message);
                    }
                    
                    break;

                default:
                    break;
            }
        }
    }
}
