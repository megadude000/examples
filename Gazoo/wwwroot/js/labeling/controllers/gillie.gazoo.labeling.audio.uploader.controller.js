(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('audioUploadComponentController', audioUploadComponentController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule.audioUploadComponent', {
            controller: 'audioUploadComponentController',
            templateUrl: '/templates/labeling/audioUploadComponent.html'
        });

    audioUploadComponentController.$inject = ['$scope', '$state'];

    function audioUploadComponentController($scope, $state) {
        $scope.$state = $state;

        $scope.goToTranscriptionUploadTab = () => {
            $state.go("masterActionsModule.audioUploadComponent.transcriptionUpload", null, { reload: 'masterActionsModule.audioUploadComponent.transcriptionUpload' });
        };

        $scope.goToFCMomentUploadTab = () => {
            $state.go("masterActionsModule.audioUploadComponent.fcMomentUpload", null, { reload: 'masterActionsModule.audioUploadComponent.fcMomentUpload' });
        };

        $scope.stateHasSubstate = (substateName) => {
            return $scope.$state.current.name.indexOf(substateName) !== -1;
        };
    }
})();