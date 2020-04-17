(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('reportGeneratorController', reportGeneratorController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule.reportGenerator', {
            controller: 'reportGeneratorController',
            templateUrl: '/templates/labeling/reportGenerator.html'
        });

    reportGeneratorController.$inject = ['$scope', '$translate', 'transacriptionHttpService', 'promiseCommonService', 'moment', 'orderByFilter', 'alertService'];

    function reportGeneratorController($scope, $translate, transacriptionHttpService, promiseCommonService, moment, orderByFilter, alertService) {
        var statistic = [];
        $scope.selectedStatisticsType = null;
        $scope.statistic = orderByFilter(statistic, $scope.propertyName, $scope.reverse);
        $scope.labelingStatisticEnum = {
            General: 'general',
            Transcription: 'transcription',
            MomentsPrediction: 'momentsPrediction'
        };
        $scope.statisticsTypes = [
            { type: $scope.labelingStatisticEnum.General, name: "GENERAL_STATISTICS_LABEL" },
            { type: $scope.labelingStatisticEnum.Transcription, name: "TRANSCRIPTION_TAB" },
            { type: $scope.labelingStatisticEnum.MomentsPrediction, name: "MOMENT_PREDICTION_LABEL" }
        ];
        $scope.startDateOptions = {
            maxDate: new Date()
        };
        $scope.endDateOptions = {
            minDate: new Date()
        };
        $scope.agents = [];
        $scope.aggregatedReport = {};
        $scope.propertyName = 'date';
        $scope.reverse = true;
        $scope.csvFileName = '';

        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 25,
            totalItems: 0,
            maxVisiblePages: 7
        };

        $scope.preparedStatisticFilter = {
            "startTime": Date.now(),
            "endTime": Date.now(),
            "agentIds": [],
            "statisticsType": null
        };

        $scope.statisticFilter = {
            "startTime": Date.now(),
            "endTime": Date.now(),
            "selectedAgents": [],
            "selectedStatisticsType": null
        };

        $scope.sortBy = function (propertyName) {
            $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
            $scope.statistic = orderByFilter(statistic, $scope.propertyName, $scope.reverse);
            $scope.statisticToFile();
            $scope.pagination.currentPage = 1;
            $scope.setPage();
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedStatistic = $scope.statistic.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.startDatePopup = {
            opened: false
        };

        $scope.endDatePopup = {
            opened: false
        };

        $scope.openStartDatePopup = function () {
            $scope.startDatePopup.opened = true;
        };

        $scope.openEndDatePopup = function () {
            $scope.endDatePopup.opened = true;
        };

        $scope.changeMinEndTime = function () {
            $scope.endDateOptions.minDate = $scope.statisticFilter.startTime;
        };

        $scope.changeMaxStartTime = function () {
            $scope.startDateOptions.maxDate = $scope.statisticFilter.endTime;
        };

        $scope.initialize = function () {
            $scope.changeMinEndTime();
            $scope.changeMaxStartTime();
            $scope.statisticFilter.selectedStatisticsType = $scope.labelingStatisticEnum.General;
            $scope.generateReport();
            $scope.fetchAgents();
        };

        $scope.resetFilter = function () {
            $scope.statisticFilter.startTime = Date.now();
            $scope.statisticFilter.endTime = Date.now();
            $scope.statisticFilter.selectedAgents = [];
            $scope.statisticFilter.selectedStatisticsType = $scope.labelingStatisticEnum.General;
        };

        $scope.fetchAgents = function (searchString) {
            var request = {
                searchString: searchString,
                resultsLimit: 10
            };

            $scope.fetchAgentsPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getAgentsForReportsPromise,
                request,
                "ERROR_DURING_FETCHING_AGENTS",
                function (response) {
                    if (!response.data.length) {
                        $scope.agents = [];
                    }
                    else {
                        $scope.agents = response.data;
                    }
                });
        };

        $scope.generateReport = function () {
            var request = prepareStatisticFilterForRequest();

            if ($scope.preparedStatisticFilter.statisticsType === $scope.labelingStatisticEnum.General)
                $scope.getStatisticsPromise = promiseCommonService.createPromise(
                    transacriptionHttpService.getGeneralStatistics,
                    request,
                    "ERROR_DURING_GETTING_STATISTIC",
                    function (response) {
                        if (!response.data.statistic) {
                            alertService.showWarning("ERROR_NO_STATISTICS_FOR_SELECTED_FILTERING_LABEL");
                            return;
                        }

                        $scope.selectedStatisticsType = $scope.labelingStatisticEnum.General;
                        $scope.reverse = true;
                        $scope.propertyName = 'date';
                        setReceivedStatistics(response.data);
                    });

            if ($scope.preparedStatisticFilter.statisticsType === $scope.labelingStatisticEnum.Transcription)
                $scope.getStatisticsPromise = promiseCommonService.createPromise(
                    transacriptionHttpService.getTranscriptionStatistics,
                    request,
                    "ERROR_DURING_GETTING_STATISTIC",
                    function (response) {
                        if (!response.data.statistic) {
                            alertService.showWarning("ERROR_NO_STATISTICS_FOR_SELECTED_FILTERING_LABEL");
                            return;
                        }

                        $scope.selectedStatisticsType = $scope.labelingStatisticEnum.Transcription;
                        $scope.reverse = true;
                        $scope.propertyName = 'date';
                        setReceivedStatistics(response.data);
                    });

            if ($scope.preparedStatisticFilter.statisticsType === $scope.labelingStatisticEnum.MomentsPrediction)
                $scope.getStatisticsPromise = promiseCommonService.createPromise(
                    transacriptionHttpService.getMomentsPredictionStatistics,
                    request,
                    "ERROR_DURING_GETTING_STATISTIC",
                    function (response) {
                        if (!response.data.statistic) {
                            alertService.showWarning("ERROR_NO_STATISTICS_FOR_SELECTED_FILTERING_LABEL");
                            return;
                        }

                        $scope.selectedStatisticsType = $scope.labelingStatisticEnum.MomentsPrediction;
                        $scope.reverse = true;
                        $scope.propertyName = 'date';
                        setReceivedStatistics(response.data);
                    });
        };

        function setReceivedStatistics(data) {
            $scope.aggregatedStatistic = data.aggregatedStatistic;
            statistic = data.statistic;
            $scope.statistic = orderByFilter(statistic, $scope.propertyName, $scope.reverse);
            $scope.pagination.totalItems = data.statistic.length;
            $scope.pagination.currentPage = 1;
            $scope.statisticToFile();
            $scope.setPage();
        }

        function prepareStatisticFilterForRequest() {
            $scope.preparedStatisticFilter.agentIds = getSelectedAgents();
            $scope.preparedStatisticFilter.statisticsType = $scope.statisticFilter.selectedStatisticsType;
            $scope.preparedStatisticFilter.fromDate = moment($scope.statisticFilter.startTime).format("L");
            $scope.preparedStatisticFilter.toDate = moment($scope.statisticFilter.endTime).format("L");

            return {
                agentIds: $scope.preparedStatisticFilter.agentIds,
                fromDate: $scope.preparedStatisticFilter.fromDate,
                toDate: $scope.preparedStatisticFilter.toDate
            };
        }

        function getSelectedAgents() {
            return ($scope.statisticFilter.selectedAgents.length) ?
                $scope.statisticFilter.selectedAgents.map(selectedAgent => selectedAgent.id) :
                [];
        }

        $scope.statisticToFile = () => {
            var fDate = new Date($scope.statisticFilter.startTime);
            var tDate = new Date($scope.statisticFilter.endTime);

            if ($scope.selectedStatisticsType === $scope.labelingStatisticEnum.General)
                return generalStatisticToFile(fDate, tDate);

            if ($scope.selectedStatisticsType === $scope.labelingStatisticEnum.Transcription)
                return transcriptionStatisticToFile(fDate, tDate);

            return momentsPredictionStatisticToFile(fDate, tDate);
        };

        $scope.getCsvHeader = () => {
            if ($scope.selectedStatisticsType === $scope.labelingStatisticEnum.General)
                return [
                    $translate.instant('DATE_LABEL'),
                    $translate.instant('AGENT_NAME_LABEL'),
                    $translate.instant('TRANSCRIBED_COUNT_LABEL'),
                    $translate.instant('TRANSCRIPTION_WORKING_TIME_LABEL'),
                    $translate.instant('PREDICTION_COUNT_LABEL'),
                    $translate.instant('PREDICTION_WORKING_TIME_LABEL'),
                    $translate.instant('PROCESSING_WORKING_TIME_LABEL')
                ];

            if ($scope.selectedStatisticsType === $scope.labelingStatisticEnum.Transcription)
                return [
                    $translate.instant('DATE_LABEL'),
                    $translate.instant('AGENT_NAME_LABEL'),
                    $translate.instant('TRANSCRIBED_COUNT_LABEL'),
                    $translate.instant('TRANSCRIPTION_WORKING_TIME_LABEL'),
                    $translate.instant('TRANSCRIPTION_AUDIO_LENGTH_LABEL'),
                    $translate.instant('AVERAGE_AUDIO_LENGTH_LABEL'),
                    $translate.instant('TRANSCRIPTION_ERROR_COUNT'),
                    $translate.instant('TRANSCRIPTION_PROCESSING_SCORE_LABEL'),
                    $translate.instant('VERIFICATION_COUNT_LABEL'),
                    $translate.instant('VERIFICATION_WORKING_TIME_LABEL'),
                    $translate.instant('VERIFICATION_AUDIO_LENGTH_LABEL'),
                    $translate.instant('VERIFICATION_PROCESSING_SCORE_LABEL')
                ];

            return [
                $translate.instant('DATE_LABEL'),
                $translate.instant('AGENT_NAME_LABEL'),
                $translate.instant('PREDICTION_COUNT_LABEL'),
                $translate.instant('PREDICTION_WORKING_TIME_LABEL'),
                $translate.instant('PREDICTION_PROCESSING_SCORE_LABEL'),
                $translate.instant('VERIFICATION_COUNT_LABEL'),
                $translate.instant('VERIFICATION_WORKING_TIME_LABEL'),
                $translate.instant('VERIFICATION_PROCESSING_SCORE_LABEL')
            ];
        };

        $scope.intervalWithoutMilliseconds = (interval) => {
            if (!interval.includes('.'))
                return interval;

            return interval.split('.').slice(0, -1).join('.');
        };

        $scope.getCsvColumnOrder = () => {
            if ($scope.selectedStatisticsType === $scope.labelingStatisticEnum.General)
                return [
                    'date',
                    'agentName',
                    'transcriptionsCount',
                    'transcriptionsWorkingTime',
                    'predictionsCount',
                    'predictionsWorkingTime',
                    'processingWorkingTime'
                ];

            if ($scope.selectedStatisticsType === $scope.labelingStatisticEnum.Transcription)
                return [
                    'date',
                    'agentName',
                    'transcriptionsCount',
                    'transcriptionsWorkingTime',
                    'transcribedAudioLength',
                    'averageAudioLength',
                    'averageWordErrorRate',
                    'transcriptionsProcessingScore',
                    'verificationsCount',
                    'verificationsWorkingTime',
                    'verifiedAudiosLength',
                    'verificationsProcessingScore'
                ];

            return [
                'date',
                'agentName',
                'predictionsCount',
                'predictionsWorkingTime',
                'predictionsProcessingScore',
                'verificationsCount',
                'verificationsWorkingTime',
                'verificationsProcessingScore'
            ];
        };

        function generalStatisticToFile(fDate, tDate) {
            $scope.csvFileName = `general_statistic[${fDate.toLocaleDateString('en-GB')} to ${tDate.toLocaleDateString('en-GB')}]`;

            var mappedStatistics = $scope.statistic.map(entry => {
                return {
                    date: entry.date.toString().substring(0, 10),
                    agentName: entry.agentName,
                    transcriptionsCount: entry.transcriptionsCount,
                    transcriptionsWorkingTime: entry.transcriptionsWorkingTime,
                    predictionsCount: entry.predictionsCount,
                    predictionsWorkingTime: entry.predictionsWorkingTime,
                    processingWorkingTime: entry.processingWorkingTime
                };
            });

            var aggregatedStatistics = [{
                date: " ",
                agentName: " ",
                transcriptionsCount: $scope.aggregatedStatistic.totalTranscriptionsCount,
                transcriptionsWorkingTime: $scope.aggregatedStatistic.totalTranscriptionsWorkingTime,
                predictionsCount: $scope.aggregatedStatistic.totalPredictionsCount,
                predictionsWorkingTime: $scope.aggregatedStatistic.totalPredictionsWorkingTime,
                processingWorkingTime: $scope.aggregatedStatistic.totalProcessingWorkingTime
            }];

            return aggregatedStatistics.concat(mappedStatistics.flat(1));
        }

        function transcriptionStatisticToFile(fDate, tDate) {
            $scope.csvFileName = `transcription_statistic[${fDate.toLocaleDateString('en-GB')} to ${tDate.toLocaleDateString('en-GB')}]`;

            var mappedStatistics = $scope.statistic.map(entry => {
                return {
                    date: entry.date.toString().substring(0, 10),
                    agentName: entry.agentName,
                    transcriptionsCount: entry.transcriptionsCount,
                    transcriptionsWorkingTime: entry.transcriptionsWorkingTime,
                    transcribedAudioLength: entry.transcribedAudioLength,
                    averageAudioLength: entry.averageAudioLength,
                    averageWordErrorRate: entry.averageWordErrorRate,
                    transcriptionsProcessingScore: entry.transcriptionsProcessingScore,
                    verificationsCount: entry.verificationsCount,
                    verificationsWorkingTime: entry.verificationsWorkingTime,
                    verifiedAudiosLength: entry.verifiedAudiosLength,
                    verificationsProcessingScore: entry.verificationsProcessingScore
                };
            });

            var aggregatedStatistics = [{
                date: " ",
                agentName: " ",
                transcriptionsCount: $scope.aggregatedStatistic.totalTranscriptionsCount,
                transcriptionsWorkingTime: $scope.aggregatedStatistic.totalTranscriptionsWorkingTime,
                transcribedAudioLength: $scope.aggregatedStatistic.totalTranscribedAudioLength,
                averageAudioLength: " ",
                averageWordErrorRate: $scope.aggregatedStatistic.totalAverageWordErrorRate,
                transcriptionsProcessingScore: $scope.aggregatedStatistic.totalTranscriptionsProcessingScore,
                verificationsCount: $scope.aggregatedStatistic.totalVerificationsCount,
                verificationsWorkingTime: $scope.aggregatedStatistic.totalVerificationsWorkingTime,
                verifiedAudiosLength: $scope.aggregatedStatistic.totalVerifiedAudiosLength,
                verificationsProcessingScore: $scope.aggregatedStatistic.totalVerificationsProcessingScore
            }];

            return aggregatedStatistics.concat(mappedStatistics.flat(1));
        }

        function momentsPredictionStatisticToFile(fDate, tDate) {
            $scope.csvFileName = `moments_prediction_statistic[${fDate.toLocaleDateString('en-GB')} to ${tDate.toLocaleDateString('en-GB')}]`;

            var mappedStatistics = $scope.statistic.map(entry => {
                return {
                    date: entry.date.toString().substring(0, 10),
                    agentName: entry.agentName,
                    predictionsCount: entry.predictionsCount,
                    predictionsWorkingTime: entry.predictionsWorkingTime,
                    predictionsProcessingScore: entry.predictionsProcessingScore,
                    verificationsCount: entry.verificationsCount,
                    verificationsWorkingTime: entry.verificationsWorkingTime,
                    verificationsProcessingScore: entry.verificationsProcessingScore
                };
            });

            var aggregatedStatistics = [{
                date: " ",
                agentName: " ",
                predictionsCount: $scope.aggregatedStatistic.totalPredictionsCount,
                predictionsWorkingTime: $scope.aggregatedStatistic.totalPredictionsWorkingTime,
                predictionsProcessingScore: $scope.aggregatedStatistic.totalPredictionsProcessingScore,
                verificationsCount: $scope.aggregatedStatistic.totalVerificationsCount,
                verificationsWorkingTime: $scope.aggregatedStatistic.totalVerificationsWorkingTime,
                verificationsProcessingScore: $scope.aggregatedStatistic.totalVeriricationsProcessingScore
            }];

            return aggregatedStatistics.concat(mappedStatistics.flat(1));
        }
    }
})();