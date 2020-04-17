(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('masterActionsModuleController', masterActionsModuleController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule', {
            controller: 'masterActionsModuleController',
            templateUrl: '/templates/labeling/masterActions.html'
        });

    masterActionsModuleController.$inject = ['$scope', '$state'];

    function masterActionsModuleController($scope, $state) {
        $scope.$state = $state;

        $scope.goToAudioUploadTab = () => {
            $state.go("masterActionsModule.audioUploadComponent.transcriptionUpload", null, { reload: 'masterActionsModule.audioUploadComponent.transcriptionUpload' });
        };

        $scope.goToReportGeneratorTab = () => {
            $state.go("masterActionsModule.reportGenerator", null, { reload: 'masterActionsModule.reportGenerator' });
        };

        $scope.goToLabelsCreationToolTab = () => {
            $state.go("masterActionsModule.labelsCreationTool", null, { reload: 'masterActionsModule.labelsCreationTool' });
        };

        $scope.goToImportStatisticsTab = () => {
            $state.go("masterActionsModule.importStatistics", null, { reload: 'masterActionsModule.importStatistics' });
        };

        $scope.stateHasSubstate = (substateName) => {
            return $scope.$state.current.name.indexOf(substateName) !== -1;
        };

    }
})();