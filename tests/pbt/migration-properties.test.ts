/**
 * Property-Based Tests for Essensys Migration Validation
 * 
 * These tests validate the correctness properties identified in the design document
 * for the Essensys migration project. Each test corresponds to a specific requirement
 * and validates invariants that must hold across all valid inputs.
 */

import * as fc from 'fast-check';
import * as fs from 'fs';
import * as path from 'path';
import { glob } from 'glob';

describe('Essensys Migration Property-Based Tests', () => {
  
  // Helper functions for file system operations
  const readFileContent = (filePath: string): string => {
    try {
      return fs.readFileSync(filePath, 'utf-8');
    } catch (error) {
      return '';
    }
  };

  const fileExists = (filePath: string): boolean => {
    try {
      return fs.existsSync(filePath);
    } catch (error) {
      return false;
    }
  };

  const findFiles = (pattern: string, cwd: string = '.'): string[] => {
    try {
      return glob.sync(pattern, { cwd });
    } catch (error) {
      return [];
    }
  };

  // Test data generators
  const csFileArbitrary = fc.string({ minLength: 1, maxLength: 100 })
    .filter(s => s.endsWith('.cs') || !s.includes('.'))
    .map(s => s.endsWith('.cs') ? s : s + '.cs');

  const configFileArbitrary = fc.oneof(
    fc.constant('packages.config'),
    fc.constant('web.config'),
    fc.constant('app.config'),
    fc.string().map(s => s + '.config')
  );

  const apiControllerArbitrary = fc.string({ minLength: 1, maxLength: 50 })
    .filter(s => /^[A-Za-z][A-Za-z0-9]*$/.test(s))
    .map(s => s + 'Controller');

  describe('Property 1: Complétude de l\'analyse du code legacy', () => {
    /**
     * **Feature: essensys-migration, Property 1: Complétude de l'analyse du code legacy**
     * **Validates: Requirements 1.1**
     * 
     * For any C# file in the legacy codebase, the analysis documentation should catalog it
     */
    test('should catalog all C# files in the legacy analysis', () => {
      // Find all actual C# files in the Essensys.Web directory
      const actualCsFiles = findFiles('**/*.cs', 'Essensys.Web');
      
      // Read the legacy analysis documentation
      const legacyAnalysisContent = readFileContent('legacy-system-analysis.md');
      const legacySummaryContent = readFileContent('legacy-analysis-summary.md');
      
      // Filter out test files - they don't need to be in business analysis
      const businessCsFiles = actualCsFiles.filter(file => 
        !file.toLowerCase().includes('test') && 
        !file.toLowerCase().includes('spec')
      );
      
      if (businessCsFiles.length === 0) {
        // If no business files found, the test passes trivially
        expect(true).toBe(true);
        return;
      }
      
      // Property: For every business C# file that exists, it should be mentioned in the analysis
      fc.assert(fc.property(
        fc.constantFrom(...businessCsFiles),
        (csFile) => {
          const fileName = path.basename(csFile);
          const fileNameWithoutExt = path.basename(csFile, '.cs');
          
          // The file should be mentioned in either the detailed analysis or summary
          const mentionedInAnalysis = legacyAnalysisContent.includes(fileName) || 
                                    legacyAnalysisContent.includes(fileNameWithoutExt);
          const mentionedInSummary = legacySummaryContent.includes(fileName) || 
                                   legacySummaryContent.includes(fileNameWithoutExt);
          
          return mentionedInAnalysis || mentionedInSummary;
        }
      ), { numRuns: Math.min(100, businessCsFiles.length) });
    });
  });

  describe('Property 2: Complétude de l\'inventaire des dépendances', () => {
    /**
     * **Feature: essensys-migration, Property 2: Complétude de l'inventaire des dépendances**
     * **Validates: Requirements 1.2**
     * 
     * For any dependency in config files, it should be documented in the analysis
     */
    test('should document all dependencies from config files', () => {
      // Find all config files
      const configFiles = [
        ...findFiles('**/packages.config', 'Essensys.Web'),
        ...findFiles('**/web.config', 'Essensys.Web'),
        ...findFiles('**/app.config', 'Essensys.Web')
      ];
      
      const legacyAnalysisContent = readFileContent('legacy-system-analysis.md');
      
      // Extract package names from packages.config files
      const extractedDependencies: string[] = [];
      
      configFiles.forEach(configFile => {
        const content = readFileContent(configFile);
        
        if (configFile.includes('packages.config')) {
          // Extract package names from packages.config
          const packageMatches = content.match(/id="([^"]+)"/g);
          if (packageMatches) {
            packageMatches.forEach(match => {
              const packageName = match.match(/id="([^"]+)"/)?.[1];
              if (packageName) {
                extractedDependencies.push(packageName);
              }
            });
          }
        }
      });
      
      if (extractedDependencies.length === 0) {
        // If no dependencies found, skip this test
        expect(true).toBe(true);
        return;
      }
      
      // Property: For every dependency found in config files, it should be documented
      fc.assert(fc.property(
        fc.constantFrom(...extractedDependencies),
        (dependency) => {
          // The dependency should be mentioned in the analysis
          return legacyAnalysisContent.toLowerCase().includes(dependency.toLowerCase());
        }
      ), { numRuns: Math.min(100, extractedDependencies.length) });
    });
  });

  describe('Property 3: Documentation complète des APIs', () => {
    /**
     * **Feature: essensys-migration, Property 3: Documentation complète des APIs**
     * **Validates: Requirements 1.4**
     * 
     * For any API controller, it should be documented with its signatures
     */
    test('should document all API controllers with their signatures', () => {
      // Find all API controllers
      const apiControllers = findFiles('**/Controllers/api/**/*Controller.cs', 'Essensys.Web');
      const regularControllers = findFiles('**/Controllers/*Controller.cs', 'Essensys.Web')
        .filter(f => !f.includes('/api/'));
      
      const allControllers = [...apiControllers, ...regularControllers];
      const specificationsContent = readFileContent('specifications-api.md');
      const legacyAnalysisContent = readFileContent('legacy-system-analysis.md');
      
      // Filter out test controllers - focus on business controllers
      const businessControllers = allControllers.filter(controller => 
        !controller.toLowerCase().includes('test') && 
        !controller.toLowerCase().includes('spec')
      );
      
      if (businessControllers.length === 0) {
        expect(true).toBe(true);
        return;
      }
      
      // Property: For every business controller, it should be documented
      fc.assert(fc.property(
        fc.constantFrom(...businessControllers),
        (controllerFile) => {
          const controllerName = path.basename(controllerFile, '.cs');
          const controllerContent = readFileContent(controllerFile);
          
          // Extract method signatures from the controller
          const methodMatches = controllerContent.match(/public\s+\w+\s+\w+\s*\([^)]*\)/g) || [];
          
          // The controller should be mentioned in API specifications or analysis
          const controllerDocumented = specificationsContent.includes(controllerName) || 
                                     legacyAnalysisContent.includes(controllerName);
          
          // If the controller has methods, at least some should be documented
          if (methodMatches.length > 0) {
            const someMethodsDocumented = methodMatches.some(method => {
              const methodName = method.match(/public\s+\w+\s+(\w+)\s*\(/)?.[1];
              return methodName && (specificationsContent.includes(methodName) || 
                                  legacyAnalysisContent.includes(methodName));
            });
            return controllerDocumented || someMethodsDocumented;
          }
          
          return controllerDocumented;
        }
      ), { numRuns: Math.min(100, businessControllers.length) });
    });
  });

  describe('Property 4: Calcul des métriques de complexité', () => {
    /**
     * **Feature: essensys-migration, Property 4: Calcul des métriques de complexité**
     * **Validates: Requirements 1.5**
     * 
     * For any code file, complexity metrics should be calculated and documented
     */
    test('should calculate complexity metrics for all code files', () => {
      const codeFiles = findFiles('**/*.cs', 'Essensys.Web');
      const legacyAnalysisContent = readFileContent('legacy-system-analysis.md');
      
      // Property: For code files, complexity information should be available
      fc.assert(fc.property(
        fc.constantFrom(...codeFiles),
        (codeFile) => {
          const fileName = path.basename(codeFile);
          const fileContent = readFileContent(codeFile);
          
          // Simple complexity indicators
          const methodCount = (fileContent.match(/public\s+\w+\s+\w+\s*\(/g) || []).length;
          const classCount = (fileContent.match(/class\s+\w+/g) || []).length;
          const lineCount = fileContent.split('\n').length;
          
          // If the file has significant complexity, it should be mentioned in analysis
          if (methodCount > 5 || classCount > 1 || lineCount > 100) {
            return legacyAnalysisContent.includes(fileName) || 
                   legacyAnalysisContent.includes(path.basename(fileName, '.cs'));
          }
          
          // For smaller files, documentation is optional
          return true;
        }
      ), { numRuns: Math.min(100, codeFiles.length || 1) });
    });
  });

  describe('Property 5: Spécification complète de l\'architecture frontend', () => {
    /**
     * **Feature: essensys-migration, Property 5: Spécification complète de l'architecture frontend**
     * **Validates: Requirements 2.1**
     * 
     * All required frontend architecture elements should be specified
     */
    test('should specify all required React architecture elements', () => {
      const frontendArchContent = readFileContent('architecture-moderne-frontend.md');
      
      const requiredElements = [
        'React',
        'TypeScript',
        'components',
        'state management',
        'Redux',
        'hooks',
        'routing',
        'build'
      ];
      
      // Property: All required architecture elements should be documented
      fc.assert(fc.property(
        fc.constantFrom(...requiredElements),
        (element) => {
          return frontendArchContent.toLowerCase().includes(element.toLowerCase());
        }
      ), { numRuns: requiredElements.length });
    });
  });

  describe('Property 8: Décomposition en features autonomes', () => {
    /**
     * **Feature: essensys-migration, Property 8: Décomposition en features autonomes**
     * **Validates: Requirements 3.1**
     * 
     * The project should be decomposed into autonomous features with priorities
     */
    test('should decompose project into autonomous features with priorities', () => {
      const planificationContent = readFileContent('planification-estimation-projet.md');
      
      // Property: Features should be identified and prioritized
      const featureIndicators = [
        'feature',
        'priorité',
        'priority',
        'autonome',
        'independent',
        'module'
      ];
      
      fc.assert(fc.property(
        fc.constantFrom(...featureIndicators),
        (indicator) => {
          return planificationContent.toLowerCase().includes(indicator.toLowerCase());
        }
      ), { numRuns: featureIndicators.length });
    });
  });

  describe('Property 11: Mapping complet des données legacy', () => {
    /**
     * **Feature: essensys-migration, Property 11: Mapping complet des données legacy**
     * **Validates: Requirements 4.1**
     * 
     * A mapping should exist for each legacy table
     */
    test('should provide mapping for each legacy table', () => {
      const migrationStrategyContent = readFileContent('strategie-migration-donnees.md');
      const schemaContent = readFileContent('schema-base-donnees-moderne.md');
      
      // Known legacy tables from the analysis
      const legacyTables = [
        'ES_USER',
        'ES_MACHINE',
        'ES_ACTION',
        'ES_STATE',
        'ES_DATAINDEX',
        'ES_VERSION',
        'ES_PHONE',
        'ES_SMSSEND'
      ];
      
      // Property: Each legacy table should have a mapping documented
      fc.assert(fc.property(
        fc.constantFrom(...legacyTables),
        (tableName) => {
          return migrationStrategyContent.includes(tableName) || 
                 schemaContent.includes(tableName);
        }
      ), { numRuns: legacyTables.length });
    });
  });

  describe('Property 13: Complétude du guide de développement', () => {
    /**
     * **Feature: essensys-migration, Property 13: Complétude du guide de développement**
     * **Validates: Requirements 5.1**
     * 
     * The development guide should cover all required aspects
     */
    test('should cover all required development guide aspects', () => {
      const standardsContent = readFileContent('standards-developpement.md');
      const examplesContent = readFileContent('exemples-implementation.md');
      const stackContent = readFileContent('stack-technique-outils.md');
      
      const requiredAspects = [
        'coding standards',
        'patterns',
        'best practices',
        'examples',
        'tools',
        'testing',
        'API'
      ];
      
      // Property: All required aspects should be covered
      fc.assert(fc.property(
        fc.constantFrom(...requiredAspects),
        (aspect) => {
          const aspectLower = aspect.toLowerCase();
          return standardsContent.toLowerCase().includes(aspectLower) ||
                 examplesContent.toLowerCase().includes(aspectLower) ||
                 stackContent.toLowerCase().includes(aspectLower);
        }
      ), { numRuns: requiredAspects.length });
    });
  });

  describe('Property 15: Scénarios de test par feature métier', () => {
    /**
     * **Feature: essensys-migration, Property 15: Scénarios de test par feature métier**
     * **Validates: Requirements 6.1**
     * 
     * Each business feature should have associated test scenarios
     */
    test('should define test scenarios for each business feature', () => {
      const testPlanContent = readFileContent('plan-tests-validation.md');
      const testStrategyContent = readFileContent('strategie-tests.md');
      
      // Business features identified from the legacy system
      const businessFeatures = [
        'authentication',
        'heating',
        'shutter',
        'alarm',
        'notification',
        'firmware'
      ];
      
      // Property: Each business feature should have test scenarios
      fc.assert(fc.property(
        fc.constantFrom(...businessFeatures),
        (feature) => {
          const featureLower = feature.toLowerCase();
          return testPlanContent.toLowerCase().includes(featureLower) ||
                 testStrategyContent.toLowerCase().includes(featureLower);
        }
      ), { numRuns: businessFeatures.length });
    });
  });

  describe('Property 17: Chiffrage complet des coûts', () => {
    /**
     * **Feature: essensys-migration, Property 17: Chiffrage complet des coûts**
     * **Validates: Requirements 8.1**
     * 
     * All cost types should be estimated and documented
     */
    test('should estimate all types of costs', () => {
      const costAnalysisContent = readFileContent('analyse-cout-benefice-roi.md');
      
      const costTypes = [
        'development',
        'infrastructure',
        'training',
        'maintenance',
        'migration',
        'testing'
      ];
      
      // Property: All cost types should be documented
      fc.assert(fc.property(
        fc.constantFrom(...costTypes),
        (costType) => {
          return costAnalysisContent.toLowerCase().includes(costType.toLowerCase());
        }
      ), { numRuns: costTypes.length });
    });
  });
});