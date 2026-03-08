export interface Server {
    id: number;
    name: string;
    ipAddress: string;
    status: string;
    lastHeartbeat?: string;
}

export interface Workflow {
    id: number;
    name: string;
    packagePath: string;
    version: string;
    isActive: boolean;
}

export interface Schedule {
    id: number;
    workflowId: number;
    workflowName: string;
    name: string;
    cronExpression: string;
    isActive: boolean;
    nextRunTime?: string;
    serverIds: number[];
}

export interface Execution {
    id: number;
    scheduleId?: number;
    scheduleName?: string;
    serverId?: number;
    serverName?: string;
    workflowId: number;
    workflowName: string;
    status: string;
    startTime?: string;
    endTime?: string;
    logOutput?: string;
    triggeredBy: string;
    createdAt: string;
}
