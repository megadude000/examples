(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('audioFileModalController', audioFileModalController);

    audioFileModalController.$inject = ['$scope', '$uibModalInstance', 'modalData', 'promiseCommonService', 'examinationHttpService'];

    function audioFileModalController($scope, $uibModalInstance, modalData, promiseCommonService, examinationHttpService) {

        $scope.audioFileData = angular.copy(modalData.fileInfo);
        $scope.audioCategories = modalData.audioCategories;
        $scope.checkFileName = modalData.checkFileName;
        $scope.isFileExistsOnServer = false;
        $scope.blockWhileCheking = false;

        $scope.save = function (valid) {
            if (!valid)
                return;

            if (!modalData.validateFileInfo($scope.audioFileData))
                return;

            modalData.onSave($scope.audioFileData);

            $uibModalInstance.close();
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.checkIfFileExistOnServer = function (file) {
            if (file) {
                var request = { fileName: file };
                $scope.isFileExistsOnServer = false;
                $scope.blockWhileCheking = true;
                $scope.checkIfFileExistOnServerPromise = promiseCommonService.createPromise(
                    examinationHttpService.checkIfFileExistOnServerPromise,
                    request,
                    "ERROR_DURING_UPDATING_FILES",
                    function (response) {
                        $scope.blockWhileCheking = false;
                        $scope.isFileExistsOnServer = response.data;
                    });
            }
            else {
                $scope.isFileExistsOnServer = false;
            }
        };
    }
})();