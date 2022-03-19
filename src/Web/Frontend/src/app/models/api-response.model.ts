export class ApiResponse<T> {
    
    statusCode?: number;
    value?: T;
    isSuccess?: boolean;
    hasWarnings?: boolean;
    messages?: string[];

    constructor() {

    }
}