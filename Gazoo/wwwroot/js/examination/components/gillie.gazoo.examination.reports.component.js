(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('reportsController', reportsController);

    angular
        .module('Company.gazoo.examination')
        .component('reports', {
            controller: 'reportsController',
            templateUrl: '/templates/examination/reports.html'
        });

    reportsController.$inject = ['$scope', 'orderByFilter','$translate', 'promiseCommonService', 'examinationHttpService', 'moment', '$q', 'alertService'];

    function reportsController($scope, orderByFilter, $translate, promiseCommonService, examinationHttpService, moment, $q, alertService) {
        var examinationReports = [];
        $scope.availableInstancesToUser = [];
        $scope.ExaminationSource = ExaminationSource;
        $scope.currentExaminationSource = { source: ExaminationSource.Pebbles };
        $scope.limitElement = 10;
        $scope.csvFileName = "";
        $scope.selectSettings = { idProp: "id", displayProp: 'name', template: '{{option.name}}', enableSearch: true, scrollableHeight: '400px', scrollable: true };
        $scope.examinationReports = orderByFilter(examinationReports, $scope.propertyName, $scope.reverse);
        $scope.examinations = [];
        $scope.agents = [];
        $scope.startDateOptions;
        $scope.endDateOptions;
        $scope.propertyName = 'examinationStartDate';
        $scope.reverse = true;

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
            $scope.endDateOptions.minDate = $scope.examinationReportsFilter.startTime;
        };

        $scope.changeMaxStartTime = function () {
            $scope.startDateOptions.maxDate = $scope.examinationReportsFilter.endTime;
        };

        $scope.examinationReportsFilter = {
            "startTime": null,
            "endTime": null,
            "selectedAgents": [],
            "selectedExaminations": [],
            "selectedInstances": []
        };

        $scope.preparedExaminationReportsFilter = {
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

        $scope.initialize = function () {
            $scope.changeExaminationSource(ExaminationSource.Pebbles);
        };

        $scope.sortBy = function (propertyName) {
            $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
            $scope.examinationReports = orderByFilter(examinationReports, $scope.propertyName, $scope.reverse);
            examinationReportsToFile();
            $scope.pagination.currentPage = 1;
            $scope.setPage();
        };

        $scope.reset = function () {
            $scope.examinationReportsFilter.selectedAgents = [];
            $scope.examinationReportsFilter.selectedExaminations = [];
            $scope.examinationReportsFilter.selectedInstances = [];
            $scope.examinationReportsFilter.startTime = null;
            $scope.examinationReportsFilter.endTime = null;
            $scope.endDateOptions = {};
            $scope.startDateOptions = {};
            fetchLatestReports();
        };

        $scope.reload = function () {
            $scope.fetchExaminations();
        };

        $scope.fetchExaminations = function () {
            $scope.fetchExaminationsPromise = promiseCommonService.createPromise(
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

        $scope.fetchInstances = function () {
            $scope.fetchInstancesPromise = promiseCommonService.createPromise(
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

        $scope.changeExaminationSource = function (source) {
            $scope.currentExaminationSource.source = source;
            $scope.fetchAgents(null);
            $scope.fetchInstances();
            $scope.fetchExaminations();
            $scope.reset();
        };

        $scope.fetchAgents = function (searchString) {
            if ($scope.availableInstancesToUser) {
                $q.when($scope.fetchInstancesPromise).then(function () {
                    var request = {
                        searchString: searchString,
                        resultsLimit: 10,
                        availableInstances: $scope.availableInstancesToUser.map(availableInstance => availableInstance.id),
                        source: $scope.currentExaminationSource.source
                    };

                    $scope.fetchAgentsPromise = promiseCommonService.createPromise(
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

        $scope.getReports = function () {
            if (!prepareExaminationReportsFilterForRequest()) {
                alertService.showError("NO_SELECTED_AGENTS_IN_SELECTED_INSTANCES");
                return;
            }
            $scope.getReportsPromise = promiseCommonService.createPromise(
                examinationHttpService.getExaminationReportsPromise,
                $scope.preparedExaminationReportsFilter,
                "ERROR_DURING_FETCHING_EXAMINATION_REPORTS",
                function (response) {
                    if (!response.data.length) {
                        alertService.showWarning("ERROR_NO_REPORTS_FOR_SELECTED_FILTERING_LABEL");
                        return;
                    }

                    $scope.reverse = true;
                    $scope.propertyName = 'examinationStartDate';
                    setReceivedExaminationReports(response.data);
                });
        };

        $scope.goToReportInfoTab = function (reportId) {
            var url = $state.href("reportDetails", { "reportId": reportId });
            window.open(url, '_blank');
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedExaminationReports = $scope.examinationReports.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.getCSVHeaders = function () {
            return [
                $translate.instant('EXAMINATION_NAME'),
                $translate.instant('AGENT_NAME'),
                $translate.instant('EXAMINATION_START_DATE'),
                $translate.instant('EXAMINATION_END_DATE'),
                $translate.instant('AVERAGE_REACTION_TIME'),
                $translate.instant('EXAMINATION_ACCURACY'),
                $translate.instant('INSTANCE_NAME_LABEL')
            ];
        };

        function fetchLatestReports() {
            $q.all([$scope.fetchInstancesPromise, $scope.fetchExaminationsPromise]).then(function () {
                setSelected($scope.examinationReportsFilter.selectedExaminations, $scope.examinations);
                if ($scope.availableInstancesToUser.length > 1) {
                    $scope.examinationReportsFilter.selectedInstances.push($scope.availableInstancesToUser[0]);
                }
                $scope.examinationReportsFilter.startTime = new Date();
                $scope.examinationReportsFilter.endTime = new Date();
                $scope.getReports();
            });
        }

        function setSelected(recepient, donor) {
            donor.forEach(function (entry) {
                recepient.push({ "id": entry.id, "name": entry.name });
            });
        }

        function prepareExaminationReportsFilterForRequest() {
            var agentsTemp = prepareAgentsForExaminationReportFilter();
            var instancesTemp = prepareInstancesForExaminationReportFilter();
            if (!agentsTemp || !instancesTemp)
                return false;

            $scope.preparedExaminationReportsFilter.examinationIds = $scope.examinationReportsFilter.selectedExaminations.map(x => x.id);
            $scope.preparedExaminationReportsFilter.agentsIds = agentsTemp;
            $scope.preparedExaminationReportsFilter.instanceIds = instancesTemp;
            $scope.preparedExaminationReportsFilter.fromDate = moment($scope.examinationReportsFilter.startTime).format("L");
            $scope.preparedExaminationReportsFilter.toDate = moment($scope.examinationReportsFilter.endTime).format("L");
            $scope.preparedExaminationReportsFilter.source = $scope.currentExaminationSource.source;

            return true;
        }

        function prepareAgentsForExaminationReportFilter() {
            var temp = [];
            if ($scope.currentExaminationSource.source === ExaminationSource.Pebbles && $scope.availableInstancesToUser.length > 1) {
                $scope.examinationReportsFilter.selectedAgents.forEach(function (selectedAgent) {
                    if ($scope.examinationReportsFilter.selectedInstances.some(selectedInstance => selectedInstance.id === selectedAgent.instanceId)) {
                        temp.push(selectedAgent.id);
                    }
                });
                if ($scope.examinationReportsFilter.selectedAgents.length && !temp.length) {
                    temp = null;
                }
            }
            else {
                temp = $scope.examinationReportsFilter.selectedAgents.map(selectedAgent => selectedAgent.id);
            }

            return temp;
        }

        function prepareInstancesForExaminationReportFilter() {
            var temp = [];
            var instances = $scope.availableInstancesToUser.length > 1
                ? $scope.examinationReportsFilter.selectedInstances
                : $scope.availableInstancesToUser;

            if ($scope.currentExaminationSource.source === ExaminationSource.Pebbles && $scope.examinationReportsFilter.selectedAgents.length !== 0) {
                instances.forEach(function (instance) {
                    if ($scope.examinationReportsFilter.selectedAgents.some(selectedAgent => selectedAgent.instanceId === instance.id)) {
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

        function setReceivedExaminationReports(data) {
            examinationReports = data;
            $scope.examinationReports = data;
            $scope.pagination.totalItems = data.length;
            $scope.pagination.currentPage = 1;
            examinationReportsToFile();
            $scope.setPage();
        }

        function examinationReportsToFile() {
            var tempArray = [];
            $scope.examinationReports.forEach(function (entry) {
                tempArray.push({
                    examinationName: entry.examinationName,
                    agentName: entry.agentName,
                    examinationStartDate: new Date(entry.examinationStartDate).toLocaleString('en-GB'),
                    examinationEndDate: new Date(entry.examinationEndDate).toLocaleString('en-GB'),
                    averageReactionTime: entry.averageReactionTime,
                    examinationAccuracy: entry.examinationAccuracy + '%',
                    instanceName: entry.instanceName
                });
            });
            var fDate = new Date($scope.preparedExaminationReportsFilter.fromDate);
            var tDate = new Date($scope.preparedExaminationReportsFilter.toDate);
            $scope.csvFileName = "examination_statistic" + "[" + fDate.toLocaleDateString('en-GB') + " to " + tDate.toLocaleDateString('en-GB') + "]";
            $scope.examinationReportsToFile = tempArray;
        }
    }
})();