(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('labelingModuleController', labelingModuleController);

    labelingModuleController.$inject = ['$scope', '$state'];

    function labelingModuleController($scope, $state) {
        $scope.$state = $state;

        $scope.goToTranscriptionTab = function () {
            $state.go("transcription", null, { reload: 'transcription' });
        };

        $scope.goToRecordingLabelingTab = function () {
            $state.go("recordingLabeling", null, { reload: 'recordingLabeling' });
        };

        $scope.goToNextAmsPredictionab = function () {
            $state.go("nextAmsPrediction", null, { reload: 'nextAmsPrediction' });
        };

        $scope.goToMasterActionsModuleTab = () => {
            $state.go("masterActionsModule.audioUploadComponent.transcriptionUpload", null, { reload: 'masterActionsModule.audioUploadComponent.transcriptionUpload' });
        };
    }
})();