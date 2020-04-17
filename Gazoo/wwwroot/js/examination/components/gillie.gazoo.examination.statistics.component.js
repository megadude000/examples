(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('statisticsController', statisticsController);

    angular
        .module('Company.gazoo.examination')
        .component('statistics', {
            controller: 'statisticsController',
            templateUrl: '/templates/examination/statistics.html'
        });

    statisticsController.$inject = ['$scope', '$translate', 'promiseCommonService', 'orderByFilter', 'examinationHttpService', 'alertService', 'moment', '$q'];

    function statisticsController($scope, $translate, promiseCommonService, orderByFilter, examinationHttpService, alertService, moment, $q) {
        var examinationStatistic = [];
        $scope.availableInstancesToUser = [];
        $scope.ExaminationSource = ExaminationSource;
        $scope.currentExaminationSource = { source: ExaminationSource.Pebbles };
        $scope.examinationStatistic = orderByFilter(examinationStatistic, $scope.propertyName, $scope.reverse);
        $scope.examinationStatisticToFile = [];
        $scope.examinations = [];
        $scope.agents = [];
        $scope.statisticType = {
            ByAgent: '0',
            ByExamination: '1'
        };
        $scope.statisticBy = [
            { type: $scope.statisticType.ByAgent, name: "STATISTIC_BY_AGENT" },
            { type: $scope.statisticType.ByExamination, name: "STATISTIC_BY_EXAMINATION" }
        ];
        $scope.IsByAgent = true;
        $scope.startDateOptions;
        $scope.endDateOptions;
        $scope.propertyName = 'examinationStartDate';
        $scope.reverse = true;
        $scope.csvFileName = '';
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
            $scope.endDateOptions.minDate = $scope.examinationStatisticsFilter.startTime;
        };

        $scope.changeMaxStartTime = function () {
            $scope.startDateOptions.maxDate = $scope.examinationStatisticsFilter.endTime;
        };

        $scope.examinationStatisticsFilter = {
            "startTime": null,
            "endTime": null,
            "selectedAgents": [],
            "selectedInstances": [],
            "selectedExaminations": [],
            "selectedStatisticFilter": null
        };

        $scope.preparedExaminationStatisticsFilter = {
            "fromDate": null,
            "toDate": null,
            "agentsIds": [],
            "examinationIds": [],
            "instanceIds": [],
            "source": null
        };

        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 25,
            totalItems: 0,
            maxVisiblePages: 7
        };

        $scope.sortBy = function (propertyName) {
            $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
            $scope.examinationStatistic = orderByFilter(examinationStatistic, $scope.propertyName, $scope.reverse);
            examinationStatisticToFile();
            $scope.pagination.currentPage = 1;
            $scope.setPage();
        };

        $scope.reset = function () {
            $scope.examinationStatisticsFilter.selectedAgents = [];
            $scope.examinationStatisticsFilter.selectedExaminations = [];
            $scope.examinationStatisticsFilter.selectedInstances = [];
            $scope.examinationStatisticsFilter.selectedStatisticFilter = $scope.statisticType.ByAgent;
            $scope.examinationStatisticsFilter.startTime = null;
            $scope.examinationStatisticsFilter.endTime = null;
            $scope.endDateOptions = {};
            $scope.startDateOptions = {};
            fetchLatestStatistic();
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedExaminationStatistic = $scope.examinationStatistic.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.initialize = function () {
            $scope.changeExaminationSource(ExaminationSource.Pebbles);
        };

        $scope.reload = function () {
            $scope.fetchExaminations();
        };

        $scope.fetchInstances = function () {
            $scope.fetchInstancesForStatisticsPromise = promiseCommonService.createPromise(
                examinationHttpService.getInstances,
                null,
                "ERROR_DURING_FETCHING_INSTANCES",
                function (response) {
                    if (response.data.length === 0) {
                        $scope.availableInstancesToUser = [];
                    }
                    else {
                        $scope.availableInstancesToUser = response.data;
                    }
                });
        };

        $scope.fetchExaminations = function () {
            $scope.fetchExaminationsForStatisticsPromise = promiseCommonService.createPromise(
                examinationHttpService.getExaminationsForReportsPromise,
                null,
                "ERROR_DURING_FETCHING_EXAMINATIONS",
                function (response) {
                    if (response.data.length === 0) {
                        $scope.examinations = [];
                    }
                    else {
                        $scope.examinations = response.data;
                    }
                });
        };

        $scope.changeExaminationSource = function (source) {
            $scope.currentExaminationSource.source = source;
            $scope.fetchAgents(null);
            $scope.fetchInstances();
            $scope.fetchExaminations();
            $scope.reset();
        };

        $scope.fetchAgents = function (searchString) {
            if ($scope.availableInstancesToUser) {
                $q.when($scope.fetchInstancesForStatisticsPromise).then(function () {
                    var request = {
                        searchString: searchString,
                        resultsLimit: 10,
                        availableInstances: $scope.availableInstancesToUser.map(availableInstance => availableInstance.id),
                        source: $scope.currentExaminationSource.source
                    };

                    $scope.fetchAgentsForStatisticsPromise = promiseCommonService.createPromise(
                        examinationHttpService.getAgentsForReportsPromise,
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
                });
            }
        };

        $scope.getStatistics = function () {
            if (!prepareExaminationStatisticsFilterForRequest()) {
                alertService.showError("NO_SELECTED_AGENTS_IN_SELECTED_INSTANCES");
                return;
            }
            $scope.getExaminationStatistics = promiseCommonService.createPromise(
                examinationHttpService.getExaminationStatistics,
                $scope.preparedExaminationStatisticsFilter,
                "ERROR_DURING_FETCHING_EXAMINATION_STATISTICS",
                function (response) {
                    if (!response.data.length) {
                        alertService.showWarning("ERROR_NO_STATISTICS_FOR_SELECTED_FILTERING_LABEL");
                        return;
                    }

                    if ($scope.examinationStatisticsFilter.selectedStatisticFilter === $scope.statisticType.ByAgent) {
                        $scope.reverse = false;
                        $scope.propertyName = 'agentName';
                        $scope.IsByAgent = true;
                    }
                    else {
                        $scope.reverse = false;
                        $scope.propertyName = 'examinationName';
                        $scope.IsByAgent = false;
                    }
                    setReceivedExaminationStatisticsReports(response.data);
                });
        };

        $scope.GetCSVHeaders = function () {
            if ($scope.IsByAgent) {
                return [
                    $translate.instant('AGENT_NAME'),
                    $translate.instant('EXAMINATION_NAME'),
                    $translate.instant('AVERAGE_POSITIVE_ANSWERS_PERCENTAGE'),
                    $translate.instant('AVERAGE_FAILED_ANSWERS_PERCENTAGE'),
                    $translate.instant('AVERAGE_TIME_UPS_PERCENTAGE'),
                    $translate.instant('AVERAGE_REACTION_TIME')
                ];
            }
            else {
                return [
                    $translate.instant('EXAMINATION_NAME'),
                    $translate.instant('AGENT_NAME'),
                    $translate.instant('AVERAGE_POSITIVE_ANSWERS_PERCENTAGE'),
                    $translate.instant('AVERAGE_FAILED_ANSWERS_PERCENTAGE'),
                    $translate.instant('AVERAGE_TIME_UPS_PERCENTAGE'),
                    $translate.instant('AVERAGE_REACTION_TIME')
                ];
            }
        };

        function fetchLatestStatistic() {
            $q.all([$scope.fetchInstancesForStatisticsPromise, $scope.fetchExaminationsForStatisticsPromise]).then(function () {
                setSelected($scope.examinationStatisticsFilter.selectedExaminations, $scope.examinations);
                if ($scope.availableInstancesToUser.length > 1) {
                    $scope.examinationStatisticsFilter.selectedInstances.push($scope.availableInstancesToUser[0]);
                }
                $scope.examinationStatisticsFilter.startTime = new Date();
                $scope.examinationStatisticsFilter.endTime = new Date();
                $scope.getStatistics();
            });
        }

        function setSelected(recepient, donor) {
            donor.forEach(function (entry) {
                recepient.push({ "id": entry.id, "name": entry.name });
            });
        }

        function prepareExaminationStatisticsFilterForRequest() {
            var agentTemp = prepareAgentsForExaminationStatisticsFilter();
            var instanceTemp = prepareInstancesForExaminationStatisticsFilter();

            if (!agentTemp || !instanceTemp)
                return false;

            $scope.preparedExaminationStatisticsFilter.examinationIds = $scope.examinationStatisticsFilter.selectedExaminations.map(selectedExamination => selectedExamination.id);
            $scope.preparedExaminationStatisticsFilter.agentsIds = agentTemp;
            $scope.preparedExaminationStatisticsFilter.instanceIds = instanceTemp;
            $scope.preparedExaminationStatisticsFilter.fromDate = moment($scope.examinationStatisticsFilter.startTime).format("L");
            $scope.preparedExaminationStatisticsFilter.toDate = moment($scope.examinationStatisticsFilter.endTime).format("L");
            $scope.preparedExaminationStatisticsFilter.source = $scope.currentExaminationSource.source;

            return true;
        }

        function prepareAgentsForExaminationStatisticsFilter() {
            var temp = [];
            if ($scope.currentExaminationSource.source === ExaminationSource.Pebbles && $scope.availableInstancesToUser.length > 1) {
                $scope.examinationStatisticsFilter.selectedAgents.forEach(function (selectedAgent) {
                    if ($scope.examinationStatisticsFilter.selectedInstances.some(selectedInstance => selectedInstance.id === selectedAgent.instanceId)) {
                        temp.push(selectedAgent.id);
                    }
                });
                if ($scope.examinationStatisticsFilter.selectedAgents.length && !temp.length) {
                    temp = null;
                }
            }
            else {
                temp = $scope.examinationStatisticsFilter.selectedAgents.map(selectedAgent => selectedAgent.id);
            }

            return temp;
        }

        function prepareInstancesForExaminationStatisticsFilter() {
            var temp = [];
            var instances = $scope.availableInstancesToUser.length > 1
                ? $scope.examinationStatisticsFilter.selectedInstances
                : $scope.availableInstancesToUser;

            if ($scope.currentExaminationSource.source === ExaminationSource.Pebbles && $scope.examinationStatisticsFilter.selectedAgents.length !== 0) {
                instances.forEach(function (instance) {
                    if ($scope.examinationStatisticsFilter.selectedAgents.some(selectedAgent => selectedAgent.instanceId === instance.id)) {
                        temp.push(instance.id);
                    }
                });
                if (!temp.length) {
                    temp = null;
                }
            }
            else {
                temp = instances.map(instance => instance.id);
            }

            return temp;
        }

        function setReceivedExaminationStatisticsReports(data) {
            examinationStatistic = data;
            $scope.examinationStatistic = examinationStatistic;
            $scope.pagination.totalItems = data.length;
            $scope.pagination.currentPage = 1;
            examinationStatisticToFile();
            $scope.setPage();
        }

        function examinationStatisticToFile() {
            var tempArray = [];
            $scope.examinationStatistic.forEach(function (entry) {
                if ($scope.IsByAgent) {
                    tempArray.push({
                        agentName: entry.agentName,
                        examinationName: entry.examinationName,
                        averagePositiveAnswersPercentage: entry.averagePositiveAnswersPercentage.toFixed(2),
                        averageFailedAnswersPercentage: entry.averageFailedAnswersPercentage.toFixed(2),
                        averageTimeUpsPercentage: entry.averageTimeUpsPercentage.toFixed(2),
                        averageReationTime: entry.averageReationTime.toFixed(2)
                    });
                }
                else {
                    tempArray.push({
                        examinationName: entry.examinationName,
                        agentName: entry.agentName,
                        averagePositiveAnswersPercentage: entry.averagePositiveAnswersPercentage.toFixed(2),
                        averageFailedAnswersPercentage: entry.averageFailedAnswersPercentage.toFixed(2),
                        averageTimeUpsPercentage: entry.averageTimeUpsPercentage.toFixed(2),
                        averageReationTime: entry.averageReationTime.toFixed(2)
                    });
                }

            });
            var fDate = new Date($scope.preparedExaminationStatisticsFilter.fromDate);
            var tDate = new Date($scope.preparedExaminationStatisticsFilter.toDate);
            $scope.csvFileName = "examination_statistic" + ($scope.IsByAgent ? "by_agents" : "by_examinations") + "[" + fDate.toLocaleDateString('en-GB') + " to " + tDate.toLocaleDateString('en-GB') + "]";
            $scope.examinationStatisticToFile = tempArray;
        }
    }
})();