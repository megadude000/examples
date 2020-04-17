(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('transacriptionAudioUploaderController', transacriptionAudioUploaderController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule.audioUploadComponent.transcriptionUpload', {
            controller: 'transacriptionAudioUploaderController',
            templateUrl: '/templates/labeling/transcriptionAudioUploader.html'
        });

    transacriptionAudioUploaderController.$inject = ['$q', '$scope', 'FileUploader', 'transacriptionHttpService', 'promiseCommonService'];

    function transacriptionAudioUploaderController($q, $scope, FileUploader, transacriptionHttpService, promiseCommonService) {
        var importNumber = null;
        $scope.comment = "";
        $scope.priorityLevels = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        $scope.priority = $scope.priorityLevels[0];
        $scope.instanceId = 0;
        $scope.campaignId = 0;
        $scope.labelingTypes = [];
        $scope.selectSettings = { idProp: 'id', displayProp: 'name', template: '{{option.name}}', scrollableHeight: '400px', scrollable: true };
        $scope.selectedLabelingTypes = [];
        $scope.uploader = new FileUploader({});
        var labelingType = LabelingType;

        $scope.initialize = function () {
            getLabelingTypes();
        };

        $scope.uploadAll = function () {
            $scope.uploader.url = 'api/Transcription/SaveAudio/' + $scope.instanceId;
            $scope.uploader.uploadAll();
        };

        $scope.cleanQueue = () => {
            $scope.uploader.clearQueue();
            $scope.instanceId = 0;
            $scope.campaignId = 0;
        };

        $scope.uploader.onBeforeUploadItem = function (item) {
            item.url = 'api/Transcription/SaveAudio/' + $scope.instanceId + '/' + $scope.campaignId + '/' + importNumber;
        };

        $scope.uploader.onCompleteAll = function () {
            $scope.uploader.clearQueue();
            importNumber = null;
        };

        $scope.uploadAll = function () {
            if (!importNumber)
                getNewImportNumber();

            $q.when($scope.getImportNumberPromice).then(function () {
                $scope.uploader.uploadAll();
            });
        };

        function getLabelingTypes() {
            $scope.getLabelingTypesPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getLabelGroups,
                null,
                "ERROR",
                function (response) {
                    $scope.labelingTypes = response.data;
                });
        }

        function getNewImportNumber() {
            var assignedLabelGroups = $scope.selectedLabelingTypes.map(item => item.id);
          
            var request = {
                comment: $scope.comment,
                priority: $scope.priority,
                assignedLabels: assignedLabelGroups.length ? assignedLabelGroups : null,
                type: labelingType.Transcription
            };

            $scope.getImportNumberPromice = promiseCommonService.createPromise(
                transacriptionHttpService.getNewImportNumber,
                request,
                "ERROR",
                function (response) {
                    importNumber = response.data;
                });
        }
    }
})();