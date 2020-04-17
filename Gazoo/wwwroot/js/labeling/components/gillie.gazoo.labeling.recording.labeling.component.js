(() => {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('recordingLabelingController', recordingLabelingController);

    angular
        .module('Company.gazoo.labeling')
        .component('recordingLabeling', {
            controller: 'recordingLabelingController',
            templateUrl: '/templates/labeling/recordingLabeling.html'
        });

    recordingLabelingController.$inject = ['$scope', 'promiseCommonService', 'alertService', 'recordingSharedService', '$translate', '$ngBootbox'];

    function recordingLabelingController($scope, promiseCommonService, alertService, recordingSharedService, $translate, $ngBootbox) {
        $scope.setSearchingState = () => {
            $scope.searchingRecording = true;
        };

        $scope.getRecording = (recordingId) => {
            if (UserClaims.some(claim => claim === 'ViewRecordings'))
                return promiseCommonService.createPromise(recordingSharedService.getCallAudioStatus, recordingId, "ERROR_INVALID_RECORDING_ID", (response) => {
                    if (!response) {
                        alertService.showError("ERROR_INVALID_RECORDING_ID");
                        return;
                    }
                    else if (response.data !== CallAudioStatus.Synced) {
                        alertService.showError("ERROR_AUDIO_IS_NOT_SYNCED");
                        return;
                    }
                    $scope.searchingRecording = false;
                    $scope.recordingTabTitle = $translate.instant('SELECTED_RECORDING_ID').format(recordingId);
                });
            else
                alertService.showWarning("NOT_PERMISSION_TO_VIEW_RECORDINGS_WARNING");
        };

        $scope.cancel = () => {
            $ngBootbox.confirm($translate.instant("UNSAVED_DATA_LOST_WARNING"))
                .then(() => {
                    $scope.setSearchingState();
                });
        };
    }
})();