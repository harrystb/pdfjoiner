# PDFJoiner
## Overview
A tool to manipulate PDFs by adding selecting page ranges from PDFs which are then generated into a new PDF. This functionallity enables joining two PDFs, cutting pages out, multiplying pages, adding a range from one PDF into another, and many other uses.

## Installation
Extract the executable from the release zip file. The executable is a portable version, and does not require an installation.

## Usage
/TODO: Update images bellow with new layout.../

When the application is first run, the following window will be displayed.
![Main Window](/Resources/screenshots/start-window.png)

### Overview
The screenshot below shows each of the control areas of the program.
![Controls](/Resources/screenshots/panels.png)


### Status Text
The latest status update is shown in the *Status Text*, this will display the last successful action, the current running action or last error encountered.
#### Successful Action Status
Successful action status is shown in Green.
![Successful Status](/Resources/screenshots/success-status.png)
#### In Progress Action Status
In progress actions are shown in Orange.
![Successful Status](/Resources/screenshots/generating-status.png)
#### Error Status
Errors are shown as Red.
![Successful Status](/Resources/screenshots/error-status.png)

### Document List
The document list displays the documents that are currently added, as well as allowing the addition of more documents, selecting documents and reset to an empty list.
#### Adding Documents
To add a document, press the *Add Document* button. This will open a *Open File* dialog box to allow the selection of one or more PDFs. The screenshow below shows the document panel when there have been two documents added to the list. When added to the list, each document will be given a unique character used to identify the document, *A* and *B* in the screenshot below..
![Document List](/Resources/screenshots/2-documents.png)
#### Selecting Documents
To select a document, click on the row the document in the document list. The screenshot below shows the second document identified by *B* has been selected.
![Document List](/Resources/screenshots/2-documents.png)
#### Resetting the document list
The *Reset* button removes all document from the list, and clears the generation string ready for new documents to be added.

### Document Details
The document details area shows information about the selected document. The details include the name of the document, the path to the document and the number of pages. There is also a text box which can be used to add certain pages of the document into the generation string. The pages can be ranges, such as *1-3* for pages 1,2 and 3, or individually specified, such as *1,2,3* for pages 1, 2 and 3, or any combiantion of the two method. Once the some pages have been added to the text box, press the *Add Pages* button to add the pages into the generation string.
![Add Pages](/Resources/screenshots/add-pages-document-details.png)

### Generation Panel
The generation panel has a text box which displayed the current format to join the documents together, called the *Join Format*. This is a text representation of how the document will be generated, showing the order and the source of the pages to be added to the document. For example the screenshot below shows a format string of *B1-5, A2*, this will create a document with the first five pages of document *B* and the second page of document *A*. This string can be filled in manually, or by selecting documents and adding pages through the *Document Details* panel.
![Add Pages](/Resources/screenshots/generation-panel.png)
The *Show Terminal* button will show the terminal of a generation in progress. This can be used if the generation is taking longer than expected to check for any errors that may occur in the generation. The cancel button will stop the generation of a document.



