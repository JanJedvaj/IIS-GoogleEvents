import axios from 'axios';
import { apiClient } from './client';
import type {
  EventSearchResponse,
  EventSearchResultDto,
  StandardResponse,
  XmlExportResultDto,
  XmlValidationResponse,
} from '../types/models';

const SOAP_ENDPOINT = '/soap/EventSoapService.asmx';
const SOAP_NS = 'http://iis.algebra.hr/soap';
const SOAP_ACTION_PREFIX = 'http://iis.algebra.hr/soap/IEventSoapService/';
const XSI_NS = 'http://www.w3.org/2001/XMLSchema-instance';

function escapeXml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

function buildEnvelope(bodyXml: string): string {
  return `<?xml version="1.0" encoding="utf-8"?><soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"><soap:Body>${bodyXml}</soap:Body></soap:Envelope>`;
}

function extractFaultString(raw: string): string | null {
  const doc = new DOMParser().parseFromString(raw, 'text/xml');
  const fault = Array.from(doc.getElementsByTagName('*')).find(
    (element) => element.localName === 'faultstring',
  );
  return fault?.textContent ?? null;
}

async function postSoap(operation: string, bodyXml: string): Promise<Document> {
  try {
    const response = await apiClient.post<string>(SOAP_ENDPOINT, buildEnvelope(bodyXml), {
      headers: {
        'Content-Type': 'text/xml',
        SOAPAction: `${SOAP_ACTION_PREFIX}${operation}`,
      },
      responseType: 'text',
    });
    return new DOMParser().parseFromString(response.data, 'text/xml');
  } catch (error) {
    if (axios.isAxiosError(error) && error.response) {
      const raw = typeof error.response.data === 'string' ? error.response.data : '';
      const message =
        extractFaultString(raw) ?? `SOAP zahtjev nije uspio (status ${error.response.status}).`;
      error.response.data = { message } satisfies Partial<StandardResponse<unknown>>;
    }
    throw error;
  }
}

function resultElement(doc: Document, localName: string): Element | null {
  return doc.getElementsByTagNameNS(SOAP_NS, localName)[0] ?? null;
}

function childByLocalName(parent: Element, localName: string): Element | null {
  return Array.from(parent.children).find((child) => child.localName === localName) ?? null;
}

function childrenByLocalName(parent: Element, localName: string): Element[] {
  return Array.from(parent.children).filter((child) => child.localName === localName);
}

function textOf(element: Element | null): string | null {
  if (!element) return null;
  if (element.getAttributeNS(XSI_NS, 'nil') === 'true') return null;
  return element.textContent;
}

function parseEventSearchResultDto(element: Element): EventSearchResultDto {
  return {
    googleEventId: textOf(childByLocalName(element, 'GoogleEventId')) ?? '',
    summary: textOf(childByLocalName(element, 'Summary')) ?? '',
    description: textOf(childByLocalName(element, 'Description')),
    location: textOf(childByLocalName(element, 'Location')),
    start: textOf(childByLocalName(element, 'Start')) ?? '',
    end: textOf(childByLocalName(element, 'End')) ?? '',
    status: textOf(childByLocalName(element, 'Status')) ?? '',
    matchedIn: textOf(childByLocalName(element, 'MatchedIn')) ?? '',
  };
}

export async function searchEvents(searchTerm: string): Promise<EventSearchResponse> {
  const doc = await postSoap(
    'SearchEvents',
    `<SearchEvents xmlns="${SOAP_NS}"><searchTerm>${escapeXml(searchTerm)}</searchTerm></SearchEvents>`,
  );
  const result = resultElement(doc, 'SearchEventsResult');
  const resultsNode = result ? childByLocalName(result, 'Results') : null;
  const items = resultsNode ? childrenByLocalName(resultsNode, 'EventSearchResultDto') : [];

  return {
    searchTerm: result ? textOf(childByLocalName(result, 'SearchTerm')) ?? '' : '',
    totalFound: result ? Number(textOf(childByLocalName(result, 'TotalFound')) ?? '0') : 0,
    results: items.map(parseEventSearchResultDto),
    message: result ? textOf(childByLocalName(result, 'Message')) : null,
  };
}

export async function getEventCount(): Promise<number> {
  const doc = await postSoap('GetEventCount', `<GetEventCount xmlns="${SOAP_NS}" />`);
  const result = resultElement(doc, 'GetEventCountResult');
  return Number(result?.textContent ?? '0');
}

export async function validateGeneratedXml(): Promise<XmlValidationResponse> {
  const doc = await postSoap(
    'ValidateGeneratedXml',
    `<ValidateGeneratedXml xmlns="${SOAP_NS}" />`,
  );
  const result = resultElement(doc, 'ValidateGeneratedXmlResult');
  const errorsNode = result ? childByLocalName(result, 'Errors') : null;
  const errors = errorsNode
    ? Array.from(errorsNode.children)
        .filter((child) => child.localName === 'string')
        .map((child) => child.textContent ?? '')
    : [];

  return {
    isValid: result ? textOf(childByLocalName(result, 'IsValid')) === 'true' : false,
    eventCount: result ? Number(textOf(childByLocalName(result, 'EventCount')) ?? '0') : 0,
    errors,
    message: result ? textOf(childByLocalName(result, 'Message')) : null,
  };
}

export async function generateEventsXml(): Promise<StandardResponse<XmlExportResultDto>> {
  const response = await apiClient.post<StandardResponse<XmlExportResultDto>>(
    '/api/events/xml/generate',
  );
  return response.data;
}
