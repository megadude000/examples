(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('importStatisticsController', importStatisticsController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule.importStatistics', {
            controller: 'importStatisticsController',
            templateUrl: '/templates/labeling/importsStatistics.html'
        });

    importStatisticsController.$inject = ['$scope', '$translate', 'transacriptionHttpService', 'fcMomentHttpService', 'promiseCommonService', 'orderByFilter', '$uibModal', '$ngBootbox'];

    function importStatisticsController($scope, $translate, transacriptionHttpService, fcMomentHttpService, promiseCommonService, orderByFilter, $uibModal, $ngBootbox) {
        var campaignInstancePairings = [];
        var formData = {};
        var importStatistics = [];
        $scope.labelingType = LabelingType;
        $scope.importStatistics = orderByFilter(importStatistics, $scope.propertyName, $scope.reverse);
        $scope.importTypeSelectSettings = {
            idProp: 'type',
            displayProp: 'name',
            template: '{{ option.name | translate }}',
            scrollableHeight: '400px',
            scrollable: true
        };
        $scope.campaignSelectSettings = {
            idProp: 'id',
            displayProp: 'id',
            template: '{{ option.id }}',
            scrollableHeight: '400px',
            clearSearchOnClose: true,
            scrollable: true,
            enableSearch: true
        };
        $scope.importTypes = [
            { type: LabelingType.Transcription, name: "TRANSCRIPTION_TAB" },
            { type: LabelingType.FullConversationMoments, name: "MOMENT_PREDICTION_LABEL" }
        ];

        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 25,
            totalItems: 0,
            maxVisiblePages: 7
        };

        $scope.propertyName = 'date';
        $scope.reverse = true;
        $scope.csvFileName = '';

        $scope.initialize = () => {
            $scope.selectedImportTypes = [];
            $scope.instances = [];
            $scope.selectedInstance = null;
            $scope.campaigns = [];
            $scope.selectedCampaigns = [];

            $scope.getImportsInformation();
            getCampaignInstancePairing();
        };

        $scope.sortBy = function (propertyName) {
            $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
            $scope.importStatistics = orderByFilter(importStatistics, $scope.propertyName, $scope.reverse);
            $scope.importStatisticsToFile();
            $scope.pagination.currentPage = 1;
            $scope.setPage();
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedImportStatistics = $scope.importStatistics.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.transcriptionSelected = () => {
            return $scope.selectedImportTypes.length && $scope.selectedImportTypes.some((entry) => { return entry.id === LabelingType.Transcription; });
        };

        $scope.isCampaingsSelectorDisabled = () => {
            return $scope.selectedInstance === null;
        };

        $scope.getTypeAsString = (importType) => {
            return importType === LabelingType.Transcription
                ? $scope.importTypes[0].name
                : $scope.importTypes[1].name;
        };
       
        $scope.instanceSelectionChanged = () => {
            $scope.selectedCampaigns = [];
            $scope.campaigns = campaignInstancePairings.map((entry) => { if ($scope.selectedInstance && $scope.selectedInstance === entry.instanceId) return { id: entry.campaignId }; });
        };

        $scope.refreshInstanceSelection = () => {
            $scope.selectedInstance = null;
            $scope.selectedCampaigns = [];
        };

        $scope.getImportsInformation = function () {
            var request = getImportsInformationRequest();

            $scope.getImportStatisticsReportPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getImportStatistics,
                request,
                "ERROR",
                function (response) {
                    $scope.reverse = true;
                    $scope.propertyName = 'id';
                    setReceivedImportStatisticsReports(response.data);
                });
        };

        $scope.getImportFileNames = (importModel) => {
            if (importModel.importType === LabelingType.Transcription) {
                $scope.getImportFileNamesPromise = promiseCommonService.createPromise(
                    transacriptionHttpService.getImportFileNames,
                    importModel.id,
                    "ERROR",
                    function (response) {
                        downloadResults(response.data, "transcription_import_results_" + importModel.id + ".csv");
                    });
            }
            else {
                $scope.getImportFileNamesPromise = promiseCommonService.createPromise(
                    fcMomentHttpService.getImportFileNames,
                    importModel.id,
                    "ERROR",
                    function (response) {
                        downloadResults(response.data, "fc_moments_import_results_" + importModel.id + ".csv");
                    });
            }
        };

        $scope.resetRecordsVerification = (importModel) => {
            $ngBootbox.confirm($translate.instant("REVERIFY_CONFIRMATION"))
                .then(function () {
                    $scope.resetRecordsVerificationPromise = promiseCommonService.createPromise(
                        transacriptionHttpService.resetRecordsVerification,
                        importModel.id + '/' + importModel.importType,
                        "ERROR",
                        function (response) {
                            $scope.getImportsInformation();
                        });
                });
        };

        function getCampaignInstancePairing() {
            $scope.getCampaignInstancePairingPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getCampaignInstancePairing,
                null,
                "ERROR",
                function (response) {
                    $scope.instances = response.data
                        .map((entry) => { return entry.instanceId; })
                        .filter((value, index, self) => self.indexOf(value) === index);

                    campaignInstancePairings = response.data;
                });
        }

        function downloadResults(data, filename) {
            var csvContent = data.map((d) => {
                return d;
            }).join('\n');

            var blob = new Blob([csvContent], {
                type: "text/csv;charset=utf-8;"
            });
            var url = window.URL.createObjectURL(blob);
            var link = document.createElement("a");
            document.body.appendChild(link);
            link.href = url;
            link.download = decodeURIComponent(filename);
            link.click();
            document.body.removeChild(link);
        }

        $scope.editImport = (importModel) => {
            formData = {};
            formData.importModel = angular.copy(importModel);
            formData.title = $translate.instant('EDIT_LABEL');
            formData.onSave = updateImport;

            openEditImportModelForm(formData);
        };

        function updateImport(request) {
            $scope.updateImportPromise = promiseCommonService.createPromise(
                transacriptionHttpService.updateImport,
                request,
                "ERROR",
                function () {
                    $scope.getImportsInformation();
                });
        }

        function openEditImportModelForm(formData) {
            $uibModal.open({
                animation: true,
                templateUrl: 'templates/labeling/importModalTemplate.html',
                controller: 'importModalController',
                size: "md",
                resolve: {
                    formData: formData
                }
            });
        }

        function setReceivedImportStatisticsReports(data) {
            $scope.aggregatedReport = aggregateData(data);
            importStatistics = data;
            $scope.importStatistics = orderByFilter(importStatistics, $scope.propertyName, $scope.reverse);
            $scope.pagination.totalItems = data.length;
            $scope.pagination.currentPage = 1;
            $scope.importStatisticsToFile();
            $scope.setPage();
        }

        function aggregateData(data) {
            return {
                totalUploadedRecords: data.reduce((partialSum, entry) => partialSum + entry.uploadedRecords, 0),
                totalProcessedRecords: data.reduce((partialSum, entry) => partialSum + entry.processedRecords, 0),
                totalVerifiedRecords: data.reduce((partialSum, entry) => partialSum + entry.verifiedRecords, 0),
                totalUnprocessedRecords: data.reduce((partialSum, entry) => partialSum + entry.unprocessedRecords, 0)
            };
        }

        $scope.getCsvHeader = function () {
            return [
                $translate.instant('IMPORT_NUMBER_LABEL'),
                $translate.instant('IMPORT_TYPE_LABEL'),
                $translate.instant('INSTANCE_ID_LABEL'),
                $translate.instant('CAMPAIGN_ID_LABEL'),
                $translate.instant('UPLOADED_RECORDS_COUNT_LABEL'),
                $translate.instant('UNPROCESSED_RECORDS_COUNT_LABEL'),
                $translate.instant('PROCESSED_RECORDS_COUNT_LABEL'),
                $translate.instant('VERIFIED_RECORDS_COUNT_LABEL'),
                $translate.instant('IMPORT_PRIORITY_LABEL'),
                $translate.instant('IMPORT_COMMENT_LABEL')
            ];
        };

        $scope.importStatisticsToFile = function () {
            var nowDate = new Date(Date.now());

            $scope.csvFileName = `labeling_import_statistic_` + nowDate.toLocaleDateString('en-GB');

            var importStatistics = $scope.importStatistics.map(entry => {
                return {
                    id: entry.id,
                    importType: $translate.instant($scope.getTypeAsString(entry.importType)),
                    instanceId: entry.instanceId,
                    campaignId: entry.campaignId,
                    uploadedRecords: entry.uploadedRecords,
                    processedRecords: entry.processedRecords,
                    verifiedRecords: entry.verifiedRecords,
                    unprocessedRecords: entry.unprocessedRecords,
                    priority: entry.priority,
                    comment: entry.comment
                };
            });

            var aggregatedData = [{
                id: " ",
                importType: " ",
                instanceId: " ",
                campaignId: " ",
                uploadedRecords: $scope.aggregatedReport.totalUploadedRecords,
                processedRecords: $scope.aggregatedReport.totalProcessedRecords,
                verifiedRecords: $scope.aggregatedReport.totalVerifiedRecords,
                unprocessedRecords: $scope.aggregatedReport.totalUnprocessedRecords,
                priority: " ",
                comment: " "
            }];

            var concatedTranscriptionResult = importStatistics.flat(1);

            return aggregatedData.concat(concatedTranscriptionResult);
        };

        function getImportsInformationRequest() {
            return {
                selectedTypes: ($scope.selectedImportTypes.length) ? $scope.selectedImportTypes.map(item => item.id) : $scope.importTypes.map(item => item.type),
                selectedInstanceId: $scope.selectedInstance,
                selectedCampaignsIds: $scope.selectedCampaigns.map(entry => entry.id)
            };
        }
    }
})();