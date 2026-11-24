import { BootstrapContext, bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/root/app';
import { config } from './app/root/app.config.server';

const bootstrap = (context: BootstrapContext) =>
    bootstrapApplication(App, config, context);

export default bootstrap;
